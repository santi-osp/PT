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
        var documentNumber = request.DocumentNumber?.Trim() ?? "";
        if (documentNumber.Length == 0) throw new DomainValidationException("El documento es obligatorio");
        if (request.DocumentType == DocumentType.NIT && !NitValidator.IsValid(documentNumber, request.VerificationDigit?.Trim()))
            throw new DomainValidationException("NIT inválido");
        if (!await centers.ExistsActiveAsync(request.CenterId, cancellationToken))
            throw new DomainValidationException("El centro seleccionado no existe o está inactivo");
        if (!await neighborhoods.ExistsAsync(request.Address.NeighborhoodId, cancellationToken))
            throw new DomainValidationException("El barrio seleccionado no existe");
        if (await modernChannelCustomers.ExistsAsync(request.DocumentType, documentNumber, cancellationToken))
            throw new ConflictException("Creación clientes canal moderno: el documento ya pertenece al canal moderno");

        var address = request.Address;
        var customer = ResidentialCustomer.Create(
            $"RC-{Guid.NewGuid():N}"[..11].ToUpperInvariant(), treatment, request.BusinessName,
            request.ExtendedLegalName, request.Phone, request.PhoneExtension, request.MobilePhone, request.Email,
            request.DocumentType, documentNumber, request.VerificationDigit, request.Stratum, request.CenterId,
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
    public async Task<IReadOnlyList<ResidentialCustomerResponse>> HandleAsync(string? search, CustomerStatusFilter status, CancellationToken cancellationToken) =>
        (await customers.SearchAsync(search, status, cancellationToken)).Select(ResidentialCustomerResponse.From).ToList();
}

public sealed class UpdateResidentialCustomerHandler(
    IResidentialCustomerRepository customers,
    ICenterRepository centers,
    INeighborhoodRepository neighborhoods,
    TimeProvider timeProvider)
{
    public async Task<ResidentialCustomerResponse> HandleAsync(Guid id, UpdateResidentialCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customers.GetForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("El cliente a modificar no existe");
        if (customer.IsBlocked) throw new DomainConflictException("El cliente está bloqueado y no puede modificarse.");
        if (request.Address is null) throw new DomainValidationException("La dirección es obligatoria");
        if (!await centers.ExistsActiveAsync(request.CenterId, cancellationToken))
            throw new DomainValidationException("El centro seleccionado no existe o está inactivo");
        if (!await neighborhoods.ExistsAsync(request.Address.NeighborhoodId, cancellationToken))
            throw new DomainValidationException("El barrio seleccionado no existe");

        var address = request.Address;
        customer.Update(request.BusinessName, request.FirstNames, request.LastNames, request.Phone,
            request.PhoneExtension, request.MobilePhone, request.Email, request.Stratum, request.CenterId,
            current => current.Update(address.NeighborhoodId, address.IsRural, address.RuralAddress,
                address.MainRoadType, address.MainRoadNumber, address.MainRoadLetter, address.MainRoadCardinality,
                address.SecondaryRoadNumber1, address.SecondaryRoadLetter, address.SecondaryRoadCardinality1,
                address.SecondaryRoadNumber2, address.SecondaryRoadCardinality2), timeProvider.GetUtcNow());
        await customers.SaveChangesAsync(cancellationToken);
        return ResidentialCustomerResponse.From(customer);
    }
}

public sealed class RetireResidentialCustomerHandler(IResidentialCustomerRepository customers, TimeProvider timeProvider)
{
    public async Task<ResidentialCustomerResponse> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("El cliente a retirar no existe");
        customer.Retire(timeProvider.GetUtcNow());
        await customers.SaveChangesAsync(cancellationToken);
        return ResidentialCustomerResponse.From(customer);
    }
}
