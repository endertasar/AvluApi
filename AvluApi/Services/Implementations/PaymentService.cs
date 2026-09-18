using System.Data;
using System.Text.Json;
using Dapper;
using AvluApi.Auth;
using AvluApi.Infrastructure;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ISiteContext _siteContext;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IDbConnectionFactory dbFactory,
        ISiteContext siteContext)
    {
        _paymentRepo = paymentRepo;
        _dbFactory   = dbFactory;
        _siteContext = siteContext;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync(
        long? propertyId, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default)
    {
        var payments = await _paymentRepo.GetAllAsync(propertyId, dateFrom, dateTo);
        return payments.Select(MapDto);
    }

    public async Task<PaymentDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Tahsilat bulunamadı.");
        return MapDto(payment);
    }

    public async Task<CreatePaymentResponse> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default)
    {
        if (request.TargetType != "Dues" && request.TargetType != "Extra")
            throw new DomainException("targetType Dues veya Extra olmalıdır.");
        if (request.Amount <= 0)
            throw new DomainException("Tutar sıfırdan büyük olmalıdır.");

        var siteId = _siteContext.SiteId;

        using var conn = _dbFactory.CreateConnection();
        using var tx   = conn.BeginTransaction();
        try
        {
            // 1. Lock and read the target charge
            decimal chargeAmount, chargePaidAmount;
            long    chargePropertyId;
            string  chargeStatus;

            if (request.TargetType == "Dues")
            {
                var charge = await conn.QueryFirstOrDefaultAsync<DuesCharge>(new CommandDefinition(
                    """
                    SELECT Id, SiteId, PropertyId, Amount, PaidAmount, Status
                    FROM dbo.DuesCharges WITH (UPDLOCK, ROWLOCK)
                    WHERE Id = @TargetId AND SiteId = @SiteId AND IsDeleted = 0
                    """,
                    new { request.TargetId, SiteId = siteId }, tx, cancellationToken: ct));

                if (charge is null) throw new NotFoundException("Borç kaydı bulunamadı.");
                if (charge.PropertyId != request.PropertyId)
                    throw new DomainException("Borç bu mülke ait değil.");

                chargeAmount     = charge.Amount;
                chargePaidAmount = charge.PaidAmount;
                chargePropertyId = charge.PropertyId;
                chargeStatus     = charge.Status;
            }
            else // Extra
            {
                var charge = await conn.QueryFirstOrDefaultAsync<ExtraPaymentCharge>(new CommandDefinition(
                    """
                    SELECT Id, SiteId, PropertyId, Amount, PaidAmount, Status
                    FROM dbo.ExtraPaymentCharges WITH (UPDLOCK, ROWLOCK)
                    WHERE Id = @TargetId AND SiteId = @SiteId AND IsDeleted = 0
                    """,
                    new { request.TargetId, SiteId = siteId }, tx, cancellationToken: ct));

                if (charge is null) throw new NotFoundException("Ek ödeme borcu bulunamadı.");
                if (charge.PropertyId != request.PropertyId)
                    throw new DomainException("Borç bu mülke ait değil.");

                chargeAmount     = charge.Amount;
                chargePaidAmount = charge.PaidAmount;
                chargePropertyId = charge.PropertyId;
                chargeStatus     = charge.Status;
            }

            var remaining = chargeAmount - chargePaidAmount;

            // 2. Apply existing credit if requested
            decimal creditApplied = 0;
            if (request.UseCredit == true && remaining > 0)
            {
                var currentCredit = await conn.QueryFirstOrDefaultAsync<decimal?>(new CommandDefinition(
                    "SELECT CreditBalance FROM dbo.PropertyBalances WHERE PropertyId = @PropertyId AND SiteId = @SiteId",
                    new { request.PropertyId, SiteId = siteId }, tx, cancellationToken: ct)) ?? 0m;

                if (currentCredit > 0)
                    creditApplied = Math.Min(currentCredit, remaining);
            }

            // 3. Calculate amounts
            var remainingAfterCredit = remaining - creditApplied;
            var cashApplied          = Math.Min(request.Amount, remainingAfterCredit);
            var overpay              = request.Amount - cashApplied;
            var totalApplied         = creditApplied + cashApplied;
            var newPaid              = chargePaidAmount + totalApplied;
            var newStatus            = newPaid >= chargeAmount ? "Paid"
                                     : newPaid > 0            ? "Partial"
                                                              : "Pending";

            // 4. Update charge
            var updateTable = request.TargetType == "Dues" ? "dbo.DuesCharges" : "dbo.ExtraPaymentCharges";
            await conn.ExecuteAsync(new CommandDefinition(
                $"UPDATE {updateTable} SET PaidAmount = @NewPaid, Status = @NewStatus WHERE Id = @TargetId AND SiteId = @SiteId",
                new { NewPaid = newPaid, NewStatus = newStatus, request.TargetId, SiteId = siteId }, tx,
                cancellationToken: ct));

            // 5. Insert payment record (records the actual cash amount paid)
            var paymentId = await conn.ExecuteScalarAsync<long>(new CommandDefinition(
                """
                INSERT INTO dbo.Payments
                    (SiteId, PropertyId, TargetType, TargetId, Amount, Method, Note, CollectedByUserId)
                OUTPUT INSERTED.Id
                VALUES (@SiteId, @PropertyId, @TargetType, @TargetId, @Amount, @Method, @Note, @CollectedByUserId)
                """,
                new
                {
                    SiteId            = siteId,
                    request.PropertyId,
                    request.TargetType,
                    request.TargetId,
                    Amount            = request.Amount,
                    Method            = request.Method ?? "Cash",
                    request.Note,
                    CollectedByUserId = _siteContext.UserId > 0 ? (long?)_siteContext.UserId : null
                }, tx, cancellationToken: ct));

            // 6. Debit applied credit from balance
            if (creditApplied > 0)
            {
                await AdjustBalanceAsync(conn, tx, siteId, request.PropertyId, -creditApplied, ct);
                await conn.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO dbo.BalanceTransactions (SiteId, PropertyId, Direction, Amount, Source, SourceId)
                    VALUES (@SiteId, @PropertyId, 'Debit', @Amount, 'AppliedToCharge', @SourceId)
                    """,
                    new { SiteId = siteId, request.PropertyId, Amount = creditApplied, SourceId = paymentId },
                    tx, cancellationToken: ct));
            }

            // 7. Credit overpayment to balance
            if (overpay > 0)
            {
                await AdjustBalanceAsync(conn, tx, siteId, request.PropertyId, overpay, ct);
                await conn.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO dbo.BalanceTransactions (SiteId, PropertyId, Direction, Amount, Source, SourceId)
                    VALUES (@SiteId, @PropertyId, 'Credit', @Amount, 'Overpayment', @SourceId)
                    """,
                    new { SiteId = siteId, request.PropertyId, Amount = overpay, SourceId = paymentId },
                    tx, cancellationToken: ct));
            }

            // 8. Read final credit balance
            var finalCredit = await conn.QueryFirstOrDefaultAsync<decimal?>(new CommandDefinition(
                "SELECT CreditBalance FROM dbo.PropertyBalances WHERE PropertyId = @PropertyId AND SiteId = @SiteId",
                new { request.PropertyId, SiteId = siteId }, tx, cancellationToken: ct)) ?? 0m;

            // 9. Audit log
            await conn.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO dbo.AuditLogs (SiteId, UserType, UserId, Action, Entity, EntityId, Detail)
                VALUES (@SiteId, @UserType, @UserId, 'Collect', 'Payment', @EntityId, @Detail)
                """,
                new
                {
                    SiteId   = siteId,
                    UserType = _siteContext.UserType,
                    UserId   = _siteContext.UserId > 0 ? (long?)_siteContext.UserId : null,
                    EntityId = paymentId,
                    Detail   = JsonSerializer.Serialize(new
                    {
                        request.PropertyId, request.TargetType, request.TargetId,
                        request.Amount, creditApplied, overpay, newStatus
                    })
                }, tx, cancellationToken: ct));

            tx.Commit();

            return new CreatePaymentResponse(
                Id:              paymentId,
                PropertyId:      request.PropertyId,
                TargetType:      request.TargetType,
                TargetId:        request.TargetId,
                Amount:          request.Amount,
                Method:          request.Method ?? "Cash",
                PaidAt:          DateTime.UtcNow,
                ChargeStatus:    newStatus,
                ChargeRemaining: Math.Max(0, chargeAmount - newPaid),
                AppliedToCredit: overpay,
                CreditBalance:   finalCredit);
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task CancelAsync(long id, CancellationToken ct = default)
    {
        var payment = await _paymentRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Tahsilat bulunamadı.");

        if (payment.TargetType == "Dues")
        {
            using var conn = _dbFactory.CreateConnection();
            using var tx   = conn.BeginTransaction();
            try
            {
                var charge = await conn.QueryFirstOrDefaultAsync<DuesCharge>(new CommandDefinition(
                    "SELECT Id, Amount, PaidAmount FROM dbo.DuesCharges WITH (UPDLOCK, ROWLOCK) WHERE Id = @TargetId AND SiteId = @SiteId",
                    new { TargetId = payment.TargetId, SiteId = _siteContext.SiteId }, tx, cancellationToken: ct));

                if (charge is not null)
                {
                    var newPaid   = Math.Max(0, charge.PaidAmount - payment.Amount);
                    var newStatus = newPaid <= 0 ? "Pending" : newPaid < charge.Amount ? "Partial" : "Paid";
                    await conn.ExecuteAsync(new CommandDefinition(
                        "UPDATE dbo.DuesCharges SET PaidAmount = @NewPaid, Status = @NewStatus WHERE Id = @Id",
                        new { NewPaid = newPaid, NewStatus = newStatus, Id = charge.Id }, tx, cancellationToken: ct));
                }

                await conn.ExecuteAsync(new CommandDefinition(
                    "UPDATE dbo.Payments SET IsDeleted = 1 WHERE Id = @Id AND SiteId = @SiteId",
                    new { Id = id, SiteId = _siteContext.SiteId }, tx, cancellationToken: ct));

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        else // Extra
        {
            using var conn = _dbFactory.CreateConnection();
            using var tx   = conn.BeginTransaction();
            try
            {
                var charge = await conn.QueryFirstOrDefaultAsync<ExtraPaymentCharge>(new CommandDefinition(
                    "SELECT Id, Amount, PaidAmount FROM dbo.ExtraPaymentCharges WITH (UPDLOCK, ROWLOCK) WHERE Id = @TargetId AND SiteId = @SiteId",
                    new { TargetId = payment.TargetId, SiteId = _siteContext.SiteId }, tx, cancellationToken: ct));

                if (charge is not null)
                {
                    var newPaid   = Math.Max(0, charge.PaidAmount - payment.Amount);
                    var newStatus = newPaid <= 0 ? "Pending" : newPaid < charge.Amount ? "Partial" : "Paid";
                    await conn.ExecuteAsync(new CommandDefinition(
                        "UPDATE dbo.ExtraPaymentCharges SET PaidAmount = @NewPaid, Status = @NewStatus WHERE Id = @Id",
                        new { NewPaid = newPaid, NewStatus = newStatus, Id = charge.Id }, tx, cancellationToken: ct));
                }

                await conn.ExecuteAsync(new CommandDefinition(
                    "UPDATE dbo.Payments SET IsDeleted = 1 WHERE Id = @Id AND SiteId = @SiteId",
                    new { Id = id, SiteId = _siteContext.SiteId }, tx, cancellationToken: ct));

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }

    private static async Task AdjustBalanceAsync(
        IDbConnection conn, IDbTransaction tx,
        long siteId, long propertyId, decimal delta, CancellationToken ct)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            """
            MERGE dbo.PropertyBalances AS target
            USING (SELECT @PropertyId AS PropertyId) AS src ON target.PropertyId = src.PropertyId
            WHEN MATCHED THEN
                UPDATE SET CreditBalance = CreditBalance + @Delta, UpdatedAt = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (SiteId, PropertyId, CreditBalance)
                VALUES (@SiteId, @PropertyId, CASE WHEN @Delta > 0 THEN @Delta ELSE 0 END);
            """,
            new { SiteId = siteId, PropertyId = propertyId, Delta = delta }, tx, cancellationToken: ct));
    }

    private static PaymentDto MapDto(Payment p) => new()
    {
        Id                = p.Id,
        PropertyId        = p.PropertyId,
        TargetType        = p.TargetType,
        TargetId          = p.TargetId,
        Amount            = p.Amount,
        Method            = p.Method,
        Note              = p.Note,
        PaidAt            = p.PaidAt,
        CollectedByUserId = p.CollectedByUserId
    };
}
