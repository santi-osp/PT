using DataGo.Domain;

namespace DataGo.Domain.Tests;

public sealed class CustomerLifecycleTests
{
    [Fact]
    public void Person_update_recalculates_names_and_preserves_immutable_fields()
    {
        var customer = CreatePerson();
        var originalDocument = customer.DocumentNumber;
        var updatedAt = DateTimeOffset.Parse("2026-09-07T15:00:00Z");

        Update(customer, "Juan, Carlos", "Pérez, Gómez", updatedAt);

        Assert.Equal("Juan, Carlos", customer.FirstNames);
        Assert.Equal("Pérez, Gómez", customer.LastNames);
        Assert.Equal("Juan Carlos Pérez Gómez", customer.FullName);
        Assert.Equal(customer.FullName, customer.ExtendedLegalName);
        Assert.Equal(originalDocument, customer.DocumentNumber);
        Assert.Equal(updatedAt, customer.UpdatedAt);
    }

    [Fact]
    public void Blocked_customer_cannot_be_updated()
    {
        var customer = CreatePerson();
        customer.Retire(DateTimeOffset.UtcNow);

        var exception = Assert.Throws<DomainConflictException>(() => Update(customer, "Ana", "Pérez", DateTimeOffset.UtcNow));

        Assert.Equal("El cliente está bloqueado y no puede modificarse.", exception.Message);
    }

    [Fact]
    public void Retire_sets_logical_block_and_timestamps()
    {
        var customer = CreatePerson();
        var now = DateTimeOffset.Parse("2026-09-07T16:00:00Z");

        customer.Retire(now);

        Assert.True(customer.IsBlocked);
        Assert.Equal(now, customer.BlockedAt);
        Assert.Equal(now, customer.UpdatedAt);
    }

    [Fact]
    public void Repeated_retire_is_rejected_without_changing_blocked_at()
    {
        var customer = CreatePerson();
        var firstRetirement = DateTimeOffset.Parse("2026-09-07T16:00:00Z");
        customer.Retire(firstRetirement);

        Assert.Throws<DomainConflictException>(() => customer.Retire(firstRetirement.AddHours(1)));
        Assert.Equal(firstRetirement, customer.BlockedAt);
    }

    [Fact]
    public void Company_update_rejects_two_different_legal_names()
    {
        var customer = CreateCompany();

        Assert.Throws<DomainValidationException>(() => Update(customer, "Empresa Uno", "Empresa Dos", DateTimeOffset.UtcNow));
    }

    private static ResidentialCustomer CreatePerson() => ResidentialCustomer.Create("RC-0001", Treatment.Sr,
        "Cliente válido", "Ana María Pérez Gómez", "6041234", null, null, "ana@example.com", DocumentType.CC,
        "1030000001", null, 3, Guid.NewGuid(), CreateAddress, DateTimeOffset.Parse("2026-09-07T14:00:00Z"));

    private static ResidentialCustomer CreateCompany()
    {
        const string number = "800000000";
        var weights = new[] { 41, 37, 29, 23, 19, 17, 13, 7, 3 };
        var remainder = number.Select((digit, index) => (digit - '0') * weights[index]).Sum() % 11;
        var digit = remainder is 0 or 1 ? remainder : 11 - remainder;
        return ResidentialCustomer.Create("RC-0002", Treatment.Empresa, "Empresa válida", "Empresa Válida SAS",
            "6041234", null, null, "legal@empresa.co", DocumentType.NIT, number, digit.ToString(), 4,
            Guid.NewGuid(), CreateAddress, DateTimeOffset.Parse("2026-09-07T14:00:00Z"));
    }

    private static CustomerAddress CreateAddress(Guid customerId) => CustomerAddress.Create(customerId, Guid.NewGuid(),
        true, "Vereda La Esperanza", null, null, null, null, null, null, null, null, null);

    private static void Update(ResidentialCustomer customer, string? firstNames, string? lastNames, DateTimeOffset now) =>
        customer.Update("Cliente actualizado", firstNames, lastNames, "6045678", null, null, "nuevo@example.com", 4,
            Guid.NewGuid(), address => address.Update(Guid.NewGuid(), true, "Vereda El Porvenir", null, null, null,
                null, null, null, null, null, null), now);
}
