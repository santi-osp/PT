namespace DataGo.Domain;

public sealed class ResidentialCustomer
{
    public const string DefaultPaymentCondition = "0010 Contado";
    private ResidentialCustomer() { }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = "";
    public Treatment Treatment { get; private set; }
    public string BusinessName { get; private set; } = "";
    public string ExtendedLegalName { get; private set; } = "";
    public string FullName { get; private set; } = "";
    public string FirstNames { get; private set; } = "";
    public string LastNames { get; private set; } = "";
    public string? Phone { get; private set; }
    public string? PhoneExtension { get; private set; }
    public string? MobilePhone { get; private set; }
    public string? Email { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; } = "";
    public string? VerificationDigit { get; private set; }
    public TaxClass TaxClass { get; private set; }
    public string PaymentCondition { get; private set; } = DefaultPaymentCondition;
    public short Stratum { get; private set; }
    public Guid CenterId { get; private set; }
    public bool IsBlocked { get; private set; }
    public DateTimeOffset? BlockedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public CustomerAddress Address { get; private set; } = null!;

    public static ResidentialCustomer Create(string code, Treatment treatment, string businessName, string extendedLegalName,
        string? phone, string? phoneExtension, string? mobilePhone, string? email, DocumentType documentType,
        string documentNumber, string? verificationDigit, short stratum, Guid centerId, Func<Guid, CustomerAddress> createAddress,
        DateTimeOffset now)
    {
        if (!Enum.IsDefined(treatment)) throw new DomainValidationException("El tratamiento es obligatorio");
        var business = CustomerInputRules.Required(businessName, "La razón social");
        var legalName = CustomerInputRules.Required(extendedLegalName, "El nombre legal extendido");
        var normalizedPhone = CustomerInputRules.Optional(phone, "El teléfono");
        var normalizedMobile = CustomerInputRules.Optional(mobilePhone, "El celular");
        if (normalizedPhone is null && normalizedMobile is null) throw new DomainValidationException("Debe indicar teléfono o celular");
        if (centerId == Guid.Empty) throw new DomainValidationException("El centro es obligatorio");
        if (stratum is < 1 or > 6) throw new DomainValidationException("El estrato debe estar entre 1 y 6");
        var document = CustomerInputRules.Required(documentNumber, "El documento");

        if (treatment == Treatment.Empresa && documentType != DocumentType.NIT)
            throw new DomainValidationException("Empresa requiere tipo de documento NIT");
        if (treatment != Treatment.Empresa && documentType is not (DocumentType.CC or DocumentType.CE))
            throw new DomainValidationException("Sr/Sra solo permite documento CC o CE");
        if (documentType == DocumentType.NIT && string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("El email es obligatorio para NIT");
        if (documentType == DocumentType.NIT && !NitValidator.IsValid(document, verificationDigit?.Trim()))
            throw new DomainValidationException("NIT inválido");

        var names = treatment == Treatment.Empresa
            ? new ParsedName(legalName, legalName, legalName)
            : NameParser.Parse(legalName);
        var customer = new ResidentialCustomer
        {
            Id = Guid.NewGuid(), Code = CustomerInputRules.Required(code, "El código"), Treatment = treatment,
            BusinessName = business, ExtendedLegalName = legalName, FullName = names.FullName,
            FirstNames = names.FirstNames, LastNames = names.LastNames, Phone = normalizedPhone,
            PhoneExtension = CustomerInputRules.Optional(phoneExtension, "La extensión"), MobilePhone = normalizedMobile,
            Email = CustomerInputRules.Optional(email, "El email"), DocumentType = documentType, DocumentNumber = document,
            VerificationDigit = CustomerInputRules.Optional(verificationDigit, "El dígito de verificación"),
            TaxClass = treatment == Treatment.Empresa ? TaxClass.PersonaJuridica : TaxClass.PersonaNatural,
            PaymentCondition = DefaultPaymentCondition, Stratum = stratum, CenterId = centerId,
            CreatedAt = now, UpdatedAt = now
        };
        customer.Address = createAddress(customer.Id) ?? throw new DomainValidationException("La dirección es obligatoria");
        return customer;
    }
}
