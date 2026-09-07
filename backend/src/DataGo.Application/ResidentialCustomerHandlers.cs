using DataGo.Domain;

namespace DataGo.Application;

public sealed class CreateResidentialCustomerHandler(
    IResidentialCustomerRepository customers,
    ICenterRepository centers,
    INeighborhoodRepository neighborhoods,
    IModernChannelCustomerRepository modernChannelCustomers,
    TimeProvider timeProvider)
{
    public async Task<ResidentialCustomerResponse> HandleAsync(CreateResidentialCustomerRequest request, CancellationToken cancellationToken)
    {
        var treatment = request.Treatment ?? throw new DomainValidationException("El tratamiento es obligatorio");
        if (request.Address is null) throw new DomainValidationException("La dirección es obligatoria");
        if (request.DocumentType == DocumentType.NIT && !NitValidator.IsValid(request.DocumentNumber?.Trim() ?? "", request.VerificationDigit?.Trim()))
            throw new DomainValidationException("NIT inválido");
        if (!await centers.ExistsActiveAsync(request.CenterId, cancellationToken))
            throw new DomainValidationException("El centro seleccionado no existe o está inactivo");
        if (!await neighborhoods.ExistsAsync(request.Address.NeighborhoodId, cancellationToken))
            throw new DomainValidationException("El barrio seleccionado no existe");
        if (await modernChannelCustomers.ExistsAsync(request.DocumentType, request.DocumentNumber.Trim(), cancellationToken))
            throw new ConflictException("Creación clientes canal moderno: el documento ya pertenece al canal moderno");

        var address = request.Address;
        var customer = ResidentialCustomer.Create(
            $"RC-{Guid.NewGuid():N}"[..11].ToUpperInvariant(), treatment, request.BusinessName,
            request.ExtendedLegalName, request.Phone, request.PhoneExtension, request.MobilePhone, request.Email,
            request.DocumentType, request.DocumentNumber, request.VerificationDigit, request.Stratum, request.CenterId,
            customerId => CustomerAddress.Create(customerId, address.NeighborhoodId, address.IsRural,
                address.RuralAddress, address.MainRoadType, address.MainRoadNumber, address.MainRoadLetter,
                address.MainRoadCardinality, address.SecondaryRoadNumber1, address.SecondaryRoadLetter,
                address.SecondaryRoadCardinality1, address.SecondaryRoadNumber2, address.SecondaryRoadCardinality2),
            timeProvider.GetUtcNow());

        await customers.AddAsync(customer, cancellationToken);
        return ResidentialCustomerResponse.From(customer);
    }
}

public sealed class GetResidentialCustomerHandler(IResidentialCustomerRepository customers)
{
    public async Task<ResidentialCustomerResponse> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetAsync(id, cancellationToken)
            ?? throw new NotFoundException("El cliente a consultar no existe");
        return ResidentialCustomerResponse.From(customer);
    }
}

public sealed class SearchResidentialCustomersHandler(IResidentialCustomerRepository customers)
{
    public async Task<IReadOnlyList<ResidentialCustomerResponse>> HandleAsync(string? search, CancellationToken cancellationToken) =>
        (await customers.SearchAsync(search, cancellationToken)).Select(ResidentialCustomerResponse.From).ToList();
}
