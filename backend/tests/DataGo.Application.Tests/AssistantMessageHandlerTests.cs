using DataGo.Application;
using DataGo.Domain;

namespace DataGo.Application.Tests;

public sealed class AssistantMessageHandlerTests
{
    [Fact]
    public async Task Searches_by_resolved_neighborhood()
    {
        var fixture = new Fixture(new(AssistantIntent.SEARCH_CUSTOMERS,
            new(Neighborhood: "Aranjuez")));
        var result = await fixture.Messages.HandleAsync(new("Clientes de Aranjuez"), default);
        Assert.Equal(AssistantResponseType.CustomerList, result.ResponseType);
        Assert.Equal(fixture.NeighborhoodId, fixture.Customers.LastCriteria?.NeighborhoodId);
    }

    [Fact]
    public async Task Counts_with_typed_status_filter()
    {
        var fixture = new Fixture(new(AssistantIntent.COUNT_CUSTOMERS, new(Status: "activos")));
        var result = await fixture.Messages.HandleAsync(new("Cuántos clientes activos tengo"), default);
        Assert.Equal(2, result.Count);
        Assert.Equal(CustomerStatusFilter.Active, fixture.Customers.LastCriteria?.Status);
    }

    [Fact]
    public async Task Gets_exact_customer_by_code()
    {
        var fixture = new Fixture(new(AssistantIntent.GET_CUSTOMER,
            CustomerReference: new(Code: "RC-DEMO01")));
        var result = await fixture.Messages.HandleAsync(new("Busca RC-DEMO01"), default);
        Assert.Equal(AssistantResponseType.CustomerDetail, result.ResponseType);
        Assert.Equal(fixture.Customers.Items[0].Id, result.Context?.CustomerId);
    }

    [Fact]
    public async Task Returns_candidates_instead_of_choosing_ambiguous_name()
    {
        var fixture = new Fixture(new(AssistantIntent.GET_CUSTOMER,
            CustomerReference: new(Name: "Pérez")));
        var result = await fixture.Messages.HandleAsync(new("Busca a Pérez"), default);
        Assert.Equal(AssistantResponseType.Clarification, result.ResponseType);
        Assert.Equal(2, result.Customers?.Count);
    }

    [Fact]
    public async Task Update_creates_pending_action_without_mutating()
    {
        var fixture = new Fixture(new(AssistantIntent.UPDATE_CUSTOMER,
            CustomerReference: new(Code: "RC-DEMO01"),
            Fields: [new("mobilePhone", "3001234567")]));
        var result = await fixture.Messages.HandleAsync(new("Cambia el celular"), default);
        Assert.Equal(AssistantResponseType.Confirmation, result.ResponseType);
        Assert.NotNull(result.PendingAction);
        Assert.False(fixture.Customers.Updated);
    }

    [Fact]
    public async Task Retire_creates_pending_action_without_mutating()
    {
        var fixture = new Fixture(new(AssistantIntent.RETIRE_CUSTOMER,
            CustomerReference: new(Code: "RC-DEMO01")));
        var result = await fixture.Messages.HandleAsync(new("Retira RC-DEMO01"), default);
        Assert.Equal(AssistantIntent.RETIRE_CUSTOMER, result.PendingAction?.ActionType);
        Assert.False(fixture.Customers.Retired);
    }

    [Fact]
    public async Task Confirmation_executes_existing_update_use_case_once()
    {
        var fixture = new Fixture(new(AssistantIntent.UPDATE_CUSTOMER,
            CustomerReference: new(Code: "RC-DEMO01"),
            Fields: [new("mobilePhone", "3001234567")]));
        var proposal = await fixture.Messages.HandleAsync(new("Cambia el celular"), default);
        var result = await fixture.Confirmations.HandleAsync(new(proposal.PendingAction!.Token), default);
        Assert.True(fixture.Customers.Updated);
        Assert.Equal("3001234567", result.Customers![0].Customer.MobilePhone);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            fixture.Confirmations.HandleAsync(new(proposal.PendingAction.Token), default));
    }

    [Fact]
    public async Task Unknown_intent_returns_clarification()
    {
        var fixture = new Fixture(new(AssistantIntent.UNKNOWN));
        var result = await fixture.Messages.HandleAsync(new("Haz algo"), default);
        Assert.Equal(AssistantResponseType.Clarification, result.ResponseType);
    }

    [Fact]
    public async Task Provider_failure_is_not_hidden_as_domain_result()
    {
        var fixture = new Fixture(new(AssistantIntent.HELP), providerFailure: true);
        var error = await Assert.ThrowsAsync<AssistantProviderException>(() =>
            fixture.Messages.HandleAsync(new("Ayuda"), default));
        Assert.Equal("unavailable", error.Code);
    }

    private sealed class Fixture
    {
        public Guid NeighborhoodId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000134");
        public Customers Customers { get; }
        public AssistantMessageHandler Messages { get; }
        public ConfirmAssistantActionHandler Confirmations { get; }

        public Fixture(AssistantInterpretation interpretation, bool providerFailure = false)
        {
            Customers = new Customers([Create("RC-DEMO01", "Juan Pérez Gómez", NeighborhoodId),
                Create("RC-DEMO02", "Juana Pérez Gómez", NeighborhoodId)]);
            var neighborhoods = new Neighborhoods(NeighborhoodId);
            var centers = new Centers();
            var search = new SearchResidentialCustomersHandler(Customers);
            var count = new CountResidentialCustomersHandler(Customers);
            var get = new GetResidentialCustomerHandler(Customers);
            var update = new UpdateResidentialCustomerHandler(Customers, centers, neighborhoods, TimeProvider.System);
            var retire = new RetireResidentialCustomerHandler(Customers, TimeProvider.System);
            var pending = new Pending();
            Messages = new(new Model(interpretation, providerFailure), search, count, get,
                new SearchNeighborhoodsHandler(neighborhoods), new GetCentersHandler(centers), pending, TimeProvider.System);
            Confirmations = new(pending, get, update, retire, new SearchNeighborhoodsHandler(neighborhoods));
        }

        private static ResidentialCustomer Create(string code, string name, Guid neighborhoodId) =>
            ResidentialCustomer.Create(code, Treatment.Sr, "Tienda " + code, name, null, null,
                "3000000000", null, DocumentType.CC, code[^2..] + "10000000", null, 3,
                Centers.CenterId, id => CustomerAddress.Create(id, neighborhoodId, true,
                    "Vereda demo", null, null, null, null, null, null, null, null, null), DateTimeOffset.UtcNow);
    }

    private sealed class Model(AssistantInterpretation result, bool fail) : IAssistantModel
    {
        public Task<AssistantInterpretation> InterpretAsync(AssistantModelRequest request, CancellationToken token) =>
            fail ? Task.FromException<AssistantInterpretation>(new AssistantProviderException("unavailable", "falló"))
                : Task.FromResult(result);
    }

    private sealed class Customers(IReadOnlyList<ResidentialCustomer> items) : IResidentialCustomerRepository
    {
        public IReadOnlyList<ResidentialCustomer> Items { get; } = items;
        public SearchResidentialCustomerCriteria? LastCriteria { get; private set; }
        public bool Updated { get; private set; }
        public bool Retired { get; private set; }
        public Task AddAsync(ResidentialCustomer customer, CancellationToken token) => Task.CompletedTask;
        public Task<ResidentialCustomer?> GetAsync(Guid id, CancellationToken token) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
        public Task<ResidentialCustomer?> GetForUpdateAsync(Guid id, CancellationToken token) => GetAsync(id, token);
        public Task<IReadOnlyList<ResidentialCustomer>> SearchAsync(SearchResidentialCustomerCriteria criteria, CancellationToken token)
        {
            LastCriteria = criteria;
            IEnumerable<ResidentialCustomer> result = Items;
            if (criteria.NeighborhoodId.HasValue) result = result.Where(x => x.Address.NeighborhoodId == criteria.NeighborhoodId);
            if (!string.IsNullOrWhiteSpace(criteria.SearchText)) result = result.Where(x =>
                x.Code.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ||
                x.DocumentNumber.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase) ||
                x.FullName.Contains(criteria.SearchText, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult<IReadOnlyList<ResidentialCustomer>>(result.ToList());
        }
        public async Task<int> CountAsync(SearchResidentialCustomerCriteria criteria, CancellationToken token) =>
            (await SearchAsync(criteria, token)).Count;
        public Task UpdateAsync(ResidentialCustomer customer, CancellationToken token) { Updated = true; return Task.CompletedTask; }
        public Task RetireAsync(ResidentialCustomer customer, CancellationToken token) { Retired = true; return Task.CompletedTask; }
    }

    private sealed class Centers : ICenterRepository
    {
        public static Guid CenterId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000200");
        public Task<bool> ExistsActiveAsync(Guid id, CancellationToken token) => Task.FromResult(id == CenterId);
        public Task<IReadOnlyList<Center>> GetActiveAsync(CancellationToken token) =>
            Task.FromResult<IReadOnlyList<Center>>([new(CenterId, "MDE-C001", "Centro Medellín Norte")]);
    }

    private sealed class Neighborhoods(Guid id) : INeighborhoodRepository
    {
        public Task<bool> ExistsAsync(Guid value, CancellationToken token) => Task.FromResult(value == id);
        public Task<IReadOnlyList<NeighborhoodLookup>> SearchAsync(string? query, CancellationToken token) =>
            Task.FromResult<IReadOnlyList<NeighborhoodLookup>>([new(id, "Aranjuez", "Medellín", "Antioquia", "Colombia", "MDE-CENTRO")]);
    }

    private sealed class Pending : IPendingAssistantActions
    {
        private readonly Dictionary<string, StoredAssistantAction> values = [];
        public void Add(StoredAssistantAction action) => values[action.Preview.Token] = action;
        public StoredAssistantAction? Take(string token) => values.Remove(token, out var value) ? value : null;
    }
}
