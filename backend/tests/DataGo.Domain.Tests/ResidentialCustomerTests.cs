using DataGo.Domain;

namespace DataGo.Domain.Tests;

public sealed class ResidentialCustomerTests
{
    [Fact]
    public void Rejects_customer_without_phone_or_mobile() =>
        Assert.Throws<DomainValidationException>(() => Create(Treatment.Sr, DocumentType.CC, null, null));

    [Fact]
    public void Rejects_company_with_non_nit_document() =>
        Assert.Throws<DomainValidationException>(() => Create(Treatment.Empresa, DocumentType.CC, "6040000", null));

    private static ResidentialCustomer Create(Treatment treatment, DocumentType documentType, string? phone, string? mobile) =>
        ResidentialCustomer.Create("RC-TEST01", treatment, "Cliente válido", "Ana María Pérez Gómez", phone, null,
            mobile, "ana@example.com", documentType, "1030000001", null, 3, Guid.NewGuid(),
            id => CustomerAddress.Create(id, Guid.NewGuid(), true, "Vereda La Esperanza", null, null, null, null,
                null, null, null, null, null), DateTimeOffset.UtcNow);
}
