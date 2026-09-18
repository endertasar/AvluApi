using System.Text.Json;
using AvluApi.Auth;
using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class DuesService : IDuesService
{
    private readonly IDuesRepository     _duesRepo;
    private readonly IChargeRepository   _chargeRepo;
    private readonly IPropertyRepository _propertyRepo;
    private readonly IAuditLogRepository _auditRepo;
    private readonly ISiteContext        _siteContext;
    private readonly ILogger<DuesService> _logger;

    public DuesService(
        IDuesRepository duesRepo,
        IChargeRepository chargeRepo,
        IPropertyRepository propertyRepo,
        IAuditLogRepository auditRepo,
        ISiteContext siteContext,
        ILogger<DuesService> logger)
    {
        _duesRepo     = duesRepo;
        _chargeRepo   = chargeRepo;
        _propertyRepo = propertyRepo;
        _auditRepo    = auditRepo;
        _siteContext  = siteContext;
        _logger       = logger;
    }

    public async Task<IEnumerable<DuesDefinition>> GetDefinitionsAsync() =>
        await _duesRepo.GetAllAsync();

    public async Task<DuesDefinition> CreateDefinitionAsync(CreateDuesDefinitionRequest request)
    {
        var def = new DuesDefinition
        {
            PropertyType  = request.PropertyType,
            Amount        = request.Amount,
            EffectiveFrom = request.EffectiveFrom,
            Description   = request.Description
        };
        def.Id = await _duesRepo.CreateAsync(def);
        return def;
    }

    public async Task SoftDeleteDefinitionAsync(long id)
    {
        _ = await _duesRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Aidat tanımı bulunamadı.");
        await _duesRepo.SoftDeleteAsync(id);
    }

    public async Task<DuesChargeDto> CreateChargeAsync(CreateChargeRequest request)
    {
        var charge = new DuesCharge
        {
            PropertyId = request.PropertyId,
            Period     = request.Period,
            Amount     = request.Amount,
            DueDate    = request.DueDate
        };
        charge.Id = await _chargeRepo.CreateAsync(charge);

        var created = await _chargeRepo.GetByIdAsync(charge.Id);
        return MapChargeDto(created!);
    }

    public async Task<GenerateChargesResultDto> GenerateChargesAsync(int period, CancellationToken ct = default)
    {
        if (period < 200001)
            throw new DomainException("Dönem YYYYMM formatında olmalıdır (örn: 202507).");

        var siteId     = _siteContext.SiteId;
        var properties = await _propertyRepo.GetAllAsync();
        var activeProps = properties.Where(p => p.IsActive).ToList();

        int generated = 0, skipped = 0, missing = 0;

        var dueYear  = period / 100;
        var dueMonth = period % 100;
        var dueDate  = new DateTime(dueYear, dueMonth,
            Math.Min(15, DateTime.DaysInMonth(dueYear, dueMonth)), 0, 0, 0, DateTimeKind.Utc);

        foreach (var prop in activeProps)
        {
            var def = await _duesRepo.GetActiveForTypeAndPeriodAsync(siteId, prop.Type, period);
            if (def is null)
            {
                _logger.LogWarning(
                    "Aidat tanımı bulunamadı — SiteId: {SiteId}, PropertyId: {PropertyId}, Tip: {Type}, Dönem: {Period}",
                    siteId, prop.Id, prop.Type, period);

                await _auditRepo.WriteAsync(
                    action:   "DuesAccrual",
                    entity:   "MissingDefinition",
                    siteId:   siteId,
                    entityId: prop.Id,
                    userType: _siteContext.UserType,
                    userId:   _siteContext.UserId > 0 ? _siteContext.UserId : null,
                    detail:   JsonSerializer.Serialize(new { prop.Id, prop.Type, period }),
                    ct:       ct);

                missing++;
                continue;
            }

            try
            {
                var charge = new DuesCharge
                {
                    SiteId     = siteId,
                    PropertyId = prop.Id,
                    Period     = period,
                    Amount     = def.Amount,
                    DueDate    = dueDate
                };
                await _chargeRepo.CreateAsync(charge);
                generated++;
            }
            catch (Exception ex) when (IsUniqueConstraintViolation(ex))
            {
                skipped++;
            }
        }

        return new GenerateChargesResultDto
        {
            Period    = period,
            Generated = generated,
            Skipped   = skipped,
            Missing   = missing
        };
    }

    private static bool IsUniqueConstraintViolation(Exception ex) =>
        ex.Message.Contains("UQ_DuesCharges") || ex.Message.Contains("UNIQUE");

    private static DuesChargeDto MapChargeDto(DuesCharge c) => new()
    {
        Id         = c.Id,
        PropertyId = c.PropertyId,
        Period     = c.Period,
        Amount     = c.Amount,
        PaidAmount = c.PaidAmount,
        DueDate    = c.DueDate,
        Status     = c.Status,
        CreatedAt  = c.CreatedAt
    };
}
