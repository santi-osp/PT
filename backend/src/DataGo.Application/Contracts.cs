using DataGo.Domain;

namespace DataGo.Application;

public interface IResidentialCustomerRepository
{
    Task AddAsync(ResidentialCustomer customer, CancellationToken cancellationToken);
    Task<ResidentialCustomer?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ResidentialCustomer>> SearchAsync(string? search, CancellationToken cancellationToken);
}

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
