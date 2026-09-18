using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class ExtraPaymentService : IExtraPaymentService
{
    private readonly IExtraPaymentRepository _repo;
    private readonly IPropertyRepository     _propertyRepo;

    public ExtraPaymentService(IExtraPaymentRepository repo, IPropertyRepository propertyRepo)
    {
        _repo         = repo;
        _propertyRepo = propertyRepo;
    }

    public async Task<IEnumerable<ExtraPaymentDto>> GetAllAsync(CancellationToken ct = default)
    {
        var all = await _repo.GetAllAsync(ct);
        return all.Select(ep => new ExtraPaymentDto
        {
            Id                   = ep.Id,
            Name                 = ep.Name,
            AmountPerProperty    = ep.AmountPerProperty,
            TotalAmount          = ep.TotalAmount,
            InstallmentCount     = ep.InstallmentCount,
            GeneratedChargeCount = 0,
            CreatedAt            = ep.CreatedAt
        });
    }

    public async Task<ExtraPaymentDetailDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var ep = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Ek ödeme bulunamadı.");

        var charges = (await _repo.GetChargesAsync(id, null, null, ct)).ToList();

        var installments = charges
            .GroupBy(c => c.InstallmentNo)
            .OrderBy(g => g.Key)
            .Select(g => new InstallmentSummaryDto
            {
                InstallmentNo = g.Key,
                Amount        = g.First().Amount,
                PaidAmount    = g.Sum(c => c.PaidAmount),
                TotalCharges  = g.Count(),
                PaidCharges   = g.Count(c => c.Status == "Paid")
            });

        return new ExtraPaymentDetailDto
        {
            Id                = ep.Id,
            Name              = ep.Name,
            AmountPerProperty = ep.AmountPerProperty,
            TotalAmount       = ep.TotalAmount,
            InstallmentCount  = ep.InstallmentCount,
            CreatedAt         = ep.CreatedAt,
            Installments      = installments
        };
    }

    public async Task<ExtraPaymentDto> CreateAsync(CreateExtraPaymentRequest request, CancellationToken ct = default)
    {
        if (request.AmountPerProperty <= 0)
            throw new DomainException("Tutar sıfırdan büyük olmalıdır.");
        if (request.InstallmentCount < 1)
            throw new DomainException("Taksit sayısı en az 1 olmalıdır.");
        if (request.PropertyScope == "Selected" && (request.PropertyIds is null || request.PropertyIds.Count == 0))
            throw new DomainException("Seçili mülk kapsamı için en az bir mülk belirtilmelidir.");

        // Resolve target properties
        var allProperties = (await _propertyRepo.GetAllAsync()).Where(p => p.IsActive).ToList();
        List<Property> targetProperties;
        if (request.PropertyScope == "Selected")
        {
            var selectedIds = request.PropertyIds!.ToHashSet();
            targetProperties = allProperties.Where(p => selectedIds.Contains(p.Id)).ToList();
            if (targetProperties.Count == 0)
                throw new DomainException("Belirtilen mülklerden hiçbiri bu siteye ait değil veya aktif değil.");
        }
        else
        {
            targetProperties = allProperties;
        }

        // Calculate installment amounts (last installment absorbs rounding remainder)
        var installmentCount = request.InstallmentCount;
        var baseAmount       = Math.Round(request.AmountPerProperty / installmentCount, 2, MidpointRounding.ToZero);
        var remainder        = request.AmountPerProperty - baseAmount * installmentCount;

        var totalAmount = request.AmountPerProperty * targetProperties.Count;

        // Create header
        var ep = new ExtraPayment
        {
            Name              = request.Name,
            AmountPerProperty = request.AmountPerProperty,
            TotalAmount       = totalAmount,
            InstallmentCount  = installmentCount
        };
        ep.Id = await _repo.CreateAsync(ep, ct);

        // Generate installment charges per property
        var charges = new List<ExtraPaymentCharge>();
        foreach (var prop in targetProperties)
        {
            for (var i = 1; i <= installmentCount; i++)
            {
                var amount = i == installmentCount ? baseAmount + remainder : baseAmount;
                charges.Add(new ExtraPaymentCharge
                {
                    ExtraPaymentId = ep.Id,
                    PropertyId     = prop.Id,
                    InstallmentNo  = i,
                    Amount         = amount
                });
            }
        }

        var generated = await _repo.BulkInsertChargesAsync(charges, ct);

        return new ExtraPaymentDto
        {
            Id                   = ep.Id,
            Name                 = ep.Name,
            AmountPerProperty    = ep.AmountPerProperty,
            TotalAmount          = totalAmount,
            InstallmentCount     = installmentCount,
            GeneratedChargeCount = generated,
            CreatedAt            = ep.CreatedAt
        };
    }

    public async Task<IEnumerable<ExtraPaymentChargeDto>> GetChargesAsync(
        long id, long? propertyId, string? status, CancellationToken ct = default)
    {
        _ = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Ek ödeme bulunamadı.");

        var charges = await _repo.GetChargesAsync(id, propertyId, status, ct);
        return charges.Select(c => new ExtraPaymentChargeDto
        {
            Id             = c.Id,
            ExtraPaymentId = c.ExtraPaymentId,
            PropertyId     = c.PropertyId,
            InstallmentNo  = c.InstallmentNo,
            Amount         = c.Amount,
            PaidAmount     = c.PaidAmount,
            Status         = c.Status,
            DueDate        = c.DueDate,
            CreatedAt      = c.CreatedAt
        });
    }
}
