using DataGo.Domain;

namespace DataGo.Application;

public interface IResidentialCustomerRepository
{
    Task AddAsync(ResidentialCustomer customer, CancellationToken cancellationToken);
    Task<ResidentialCustomer?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<ResidentialCustomer?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ResidentialCustomer>> SearchAsync(SearchResidentialCustomerCriteria criteria, CancellationToken cancellationToken);
    Task<int> CountAsync(SearchResidentialCustomerCriteria criteria, CancellationToken cancellationToken);
    Task UpdateAsync(ResidentialCustomer customer, CancellationToken cancellationToken);
    Task RetireAsync(ResidentialCustomer customer, CancellationToken cancellationToken);
}

public enum CustomerStatusFilter { All, Active, Blocked }

public sealed record SearchResidentialCustomerCriteria(
    string? SearchText = null,
    CustomerStatusFilter Status = CustomerStatusFilter.All,
    Guid? NeighborhoodId = null,
    Guid? CenterId = null,
    short? Stratum = null,
    Treatment? Treatment = null,
    DocumentType? DocumentType = null);

public sealed record NeighborhoodLookup(
    Guid Id,
    string Name,
    string Municipality,
    string Department,
    string Country,
    string TransportZone);

public interface ICenterRepository
{
    Task<bool> ExistsActiveAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Center>> GetActiveAsync(CancellationToken cancellationToken);
}

public interface INeighborhoodRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<NeighborhoodLookup>> SearchAsync(string? query, CancellationToken cancellationToken);
}

public interface IModernChannelCustomerRepository
{
    Task<bool> ExistsAsync(DocumentType documentType, string documentNumber, CancellationToken cancellationToken);
}
