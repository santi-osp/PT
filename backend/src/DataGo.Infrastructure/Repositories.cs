using DataGo.Application;
using DataGo.Domain;
using Microsoft.EntityFrameworkCore;

namespace DataGo.Infrastructure;

internal sealed class ResidentialCustomerRepository(DataGoDbContext dbContext) : IResidentialCustomerRepository
{
    public async Task AddAsync(ResidentialCustomer customer, CancellationToken cancellationToken)
    {
        await dbContext.ResidentialCustomers.AddAsync(customer, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ResidentialCustomer?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.ResidentialCustomers.AsNoTracking().Include(x => x.Address)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ResidentialCustomer>> SearchAsync(string? search, CancellationToken cancellationToken)
    {
        var query = dbContext.ResidentialCustomers.AsNoTracking().Include(x => x.Address).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, pattern) ||
                EF.Functions.ILike(x.DocumentNumber, pattern) || EF.Functions.ILike(x.BusinessName, pattern) ||
                EF.Functions.ILike(x.FullName, pattern) || EF.Functions.ILike(x.ExtendedLegalName, pattern));
        }
        return await query.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(cancellationToken);
    }
}

internal sealed class CenterRepository(DataGoDbContext dbContext) : ICenterRepository
{
    public Task<bool> ExistsActiveAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Centers.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken);
    public async Task<IReadOnlyList<Center>> GetActiveAsync(CancellationToken cancellationToken) =>
        await dbContext.Centers.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code).ToListAsync(cancellationToken);
}

internal sealed class NeighborhoodRepository(DataGoDbContext dbContext) : INeighborhoodRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Neighborhoods.AnyAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyList<Neighborhood>> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        var neighborhoods = dbContext.Neighborhoods.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query)) neighborhoods = neighborhoods.Where(x => EF.Functions.ILike(x.Name, $"%{query.Trim()}%"));
        return await neighborhoods.OrderBy(x => x.Name).Take(100).ToListAsync(cancellationToken);
    }
}

internal sealed class ModernChannelCustomerRepository(DataGoDbContext dbContext) : IModernChannelCustomerRepository
{
    public Task<bool> ExistsAsync(DocumentType documentType, string documentNumber, CancellationToken cancellationToken) =>
        dbContext.ModernChannelCustomers.AnyAsync(x => x.DocumentType == documentType && x.DocumentNumber == documentNumber, cancellationToken);
}
