using DataGo.Application;
using DataGo.Domain;

namespace DataGo.Application.Tests;

public sealed class CreateResidentialCustomerHandlerTests
{
    [Fact]
    public async Task Rejects_modern_channel_customer_with_required_message()
    {
        var handler = new CreateResidentialCustomerHandler(new Customers(), new Centers(), new Neighborhoods(),
            new Modern(), TimeProvider.System);
        var request = new CreateResidentialCustomerRequest(Treatment.Sr, "Cliente", "Ana Pérez", "6040000", null,
            null, null, DocumentType.CC, "1010000001", null, 3, Guid.NewGuid(),
            new AddressRequest(Guid.NewGuid(), true, "Vereda La Esperanza", null, null, null, null, null, null, null, null, null));

        var exception = await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(request, CancellationToken.None));
        Assert.Contains("Creación clientes canal moderno", exception.Message);
    }

    private sealed class Customers : IResidentialCustomerRepository
    {
        public Task AddAsync(ResidentialCustomer customer, CancellationToken token) => Task.CompletedTask;
        public Task<ResidentialCustomer?> GetAsync(Guid id, CancellationToken token) => Task.FromResult<ResidentialCustomer?>(null);
        public Task<ResidentialCustomer?> GetForUpdateAsync(Guid id, CancellationToken token) => Task.FromResult<ResidentialCustomer?>(null);
        public Task<IReadOnlyList<ResidentialCustomer>> SearchAsync(string? search, CustomerStatusFilter status, CancellationToken token) => Task.FromResult<IReadOnlyList<ResidentialCustomer>>([]);
        public Task SaveChangesAsync(CancellationToken token) => Task.CompletedTask;
    }
    private sealed class Centers : ICenterRepository
    {
        public Task<bool> ExistsActiveAsync(Guid id, CancellationToken token) => Task.FromResult(true);
        public Task<IReadOnlyList<Center>> GetActiveAsync(CancellationToken token) => Task.FromResult<IReadOnlyList<Center>>([]);
    }
    private sealed class Neighborhoods : INeighborhoodRepository
    {
        public Task<bool> ExistsAsync(Guid id, CancellationToken token) => Task.FromResult(true);
        public Task<IReadOnlyList<Neighborhood>> SearchAsync(string? query, CancellationToken token) => Task.FromResult<IReadOnlyList<Neighborhood>>([]);
    }
    private sealed class Modern : IModernChannelCustomerRepository
    {
        public Task<bool> ExistsAsync(DocumentType type, string number, CancellationToken token) => Task.FromResult(true);
    }
}
