using DataGo.Domain;

namespace DataGo.Application;

public sealed record AddressRequest(
    Guid NeighborhoodId,
    bool IsRural,
    string? RuralAddress,
    string? MainRoadType,
    string? MainRoadNumber,
    string? MainRoadLetter,
    string? MainRoadCardinality,
    string? SecondaryRoadNumber1,
    string? SecondaryRoadLetter,
    string? SecondaryRoadCardinality1,
    string? SecondaryRoadNumber2,
    string? SecondaryRoadCardinality2);

public sealed record CreateResidentialCustomerRequest(
    Treatment? Treatment,
    string BusinessName,
    string ExtendedLegalName,
    string? Phone,
    string? PhoneExtension,
    string? MobilePhone,
    string? Email,
    DocumentType DocumentType,
    string DocumentNumber,
    string? VerificationDigit,
    short Stratum,
    Guid CenterId,
    AddressRequest? Address);

public sealed record UpdateResidentialCustomerRequest(
    string BusinessName,
    string? FirstNames,
    string? LastNames,
    string? Phone,
    string? PhoneExtension,
    string? MobilePhone,
    string? Email,
    short Stratum,
    Guid CenterId,
    AddressRequest? Address);

public sealed record AddressResponse(Guid NeighborhoodId, bool IsRural, string FormattedAddress);

public sealed record ResidentialCustomerResponse(
    Guid Id, string Code, Treatment Treatment, string BusinessName, string ExtendedLegalName,
    string FullName, string FirstNames, string LastNames, string? Phone, string? PhoneExtension, string? MobilePhone, string? Email,
    DocumentType DocumentType, string DocumentNumber, string? VerificationDigit, TaxClass TaxClass,
    string PaymentCondition, short Stratum, Guid CenterId, bool IsBlocked, DateTimeOffset? BlockedAt, string? Warning, AddressResponse Address,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
{
    public static ResidentialCustomerResponse From(ResidentialCustomer customer) => new(
        customer.Id, customer.Code, customer.Treatment, customer.BusinessName, customer.ExtendedLegalName,
        customer.FullName, customer.FirstNames, customer.LastNames, customer.Phone, customer.PhoneExtension, customer.MobilePhone,
        customer.Email, customer.DocumentType, customer.DocumentNumber, customer.VerificationDigit,
        customer.TaxClass, customer.PaymentCondition, customer.Stratum, customer.CenterId, customer.IsBlocked,
        customer.BlockedAt, customer.IsBlocked ? "Cliente bloqueado" : null,
        new(customer.Address.NeighborhoodId, customer.Address.IsRural, customer.Address.FormattedAddress),
        customer.CreatedAt, customer.UpdatedAt);
}

public sealed record NeighborhoodResponse(Guid Id, string Name);
public sealed record CenterResponse(Guid Id, string Code, string Name);
