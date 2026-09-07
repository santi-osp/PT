using DataGo.Domain;

namespace DataGo.Application;

public interface IResidentialCustomerRepository
{
    Task AddAsync(ResidentialCustomer customer, CancellationToken cancellationToken);
    Task<ResidentialCustomer?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<ResidentialCustomer?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ResidentialCustomer>> SearchAsync(string? search, CustomerStatusFilter status, CancellationToken cancellationToken);
    Task UpdateAsync(ResidentialCustomer customer, CancellationToken cancellationToken);
    Task RetireAsync(ResidentialCustomer customer, CancellationToken cancellationToken);
}

public enum CustomerStatusFilter { All, Active, Blocked }

public interface ICenterRepository
{
    Task<bool> ExistsActiveAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Center>> GetActiveAsync(CancellationToken cancellationToken);
}

public interface INeighborhoodRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Neighborhood>> SearchAsync(string? query, CancellationToken cancellationToken);
}

public interface IModernChannelCustomerRepository
{
    Task<bool> ExistsAsync(DocumentType documentType, string documentNumber, CancellationToken cancellationToken);
}
