using AvluApi.Infrastructure;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;

namespace AvluApi.Services.Implementations;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository        _propertyRepo;
    private readonly IPropertyBalanceRepository _balanceRepo;

    public PropertyService(IPropertyRepository propertyRepo, IPropertyBalanceRepository balanceRepo)
    {
        _propertyRepo = propertyRepo;
        _balanceRepo  = balanceRepo;
    }

    public async Task<IEnumerable<PropertyDto>> GetAllAsync(CancellationToken ct = default)
    {
        var props = await _propertyRepo.GetAllAsync();
        return props.Select(MapDto);
    }

    public async Task<PropertyDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var prop = await _propertyRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Mülk bulunamadı.");
        return MapDto(prop);
    }

    public async Task<PropertyDto> CreateAsync(CreatePropertyRequest request, CancellationToken ct = default)
    {
        var property = new Property
        {
            Block           = request.Block,
            UnitNo          = request.UnitNo,
            Type            = request.Type,
            OwnerName       = request.OwnerName,
            OwnerPhone      = request.OwnerPhone,
            ResidentName    = request.ResidentName,
            ResidentPhone   = request.ResidentPhone,
            DuesResponsible = request.DuesResponsible ?? "Owner"
        };
        property.Id = await _propertyRepo.CreateAsync(property);
        return MapDto(property);
    }

    public async Task<PropertyDto> UpdateAsync(long id, UpdatePropertyRequest request, CancellationToken ct = default)
    {
        var property = await _propertyRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Mülk bulunamadı.");

        property.Block           = request.Block;
        property.UnitNo          = request.UnitNo;
        property.Type            = request.Type;
        property.OwnerName       = request.OwnerName;
        property.OwnerPhone      = request.OwnerPhone;
        property.ResidentName    = request.ResidentName;
        property.ResidentPhone   = request.ResidentPhone;
        property.DuesResponsible = request.DuesResponsible ?? property.DuesResponsible;
        property.IsActive        = request.IsActive;
        await _propertyRepo.UpdateAsync(property);
        return MapDto(property);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        _ = await _propertyRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Mülk bulunamadı.");
        await _propertyRepo.SoftDeleteAsync(id);
    }

    public async Task<PropertyBalanceDto> GetBalanceAsync(long id, CancellationToken ct = default)
    {
        _ = await _propertyRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Mülk bulunamadı.");

        var credit       = await _balanceRepo.GetCreditBalanceAsync(id, ct);
        var transactions = await _balanceRepo.GetTransactionsAsync(id, ct);

        return new PropertyBalanceDto
        {
            PropertyId    = id,
            CreditBalance = credit,
            Transactions  = transactions.Select(t => new BalanceTxDto(
                t.Direction, t.Amount, t.Source, t.Note, t.CreatedAt))
        };
    }

    private static PropertyDto MapDto(Property p) => new()
    {
        Id              = p.Id,
        Block           = p.Block,
        UnitNo          = p.UnitNo,
        Type            = p.Type,
        OwnerName       = p.OwnerName,
        OwnerPhone      = p.OwnerPhone,
        ResidentName    = p.ResidentName,
        ResidentPhone   = p.ResidentPhone,
        DuesResponsible = p.DuesResponsible,
        IsActive        = p.IsActive,
        CreatedAt       = p.CreatedAt
    };
}
