namespace DataGo.Domain;

public sealed class CustomerAddress
{
    private CustomerAddress() { }
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid NeighborhoodId { get; private set; }
    public bool IsRural { get; private set; }
    public string? RuralAddress { get; private set; }
    public string? MainRoadType { get; private set; }
    public string? MainRoadNumber { get; private set; }
    public string? MainRoadLetter { get; private set; }
    public string? MainRoadCardinality { get; private set; }
    public string? SecondaryRoadNumber1 { get; private set; }
    public string? SecondaryRoadLetter { get; private set; }
    public string? SecondaryRoadCardinality1 { get; private set; }
    public string? SecondaryRoadNumber2 { get; private set; }
    public string? SecondaryRoadCardinality2 { get; private set; }
    public string FormattedAddress { get; private set; } = "";

    public static CustomerAddress Create(Guid customerId, Guid neighborhoodId, bool isRural, string? ruralAddress,
        string? mainRoadType, string? mainRoadNumber, string? mainRoadLetter, string? mainRoadCardinality,
        string? secondaryRoadNumber1, string? secondaryRoadLetter, string? secondaryRoadCardinality1,
        string? secondaryRoadNumber2, string? secondaryRoadCardinality2)
    {
        if (neighborhoodId == Guid.Empty) throw new DomainValidationException("El barrio es obligatorio");
        var address = new CustomerAddress { Id = Guid.NewGuid(), CustomerId = customerId, NeighborhoodId = neighborhoodId, IsRural = isRural };
        if (isRural)
        {
            address.RuralAddress = CustomerInputRules.Required(ruralAddress, "La dirección rural");
            address.FormattedAddress = address.RuralAddress;
            return address;
        }

        address.MainRoadType = CustomerInputRules.Required(mainRoadType, "El tipo de vía principal");
        address.MainRoadNumber = CustomerInputRules.Required(mainRoadNumber, "El número de vía principal");
        address.SecondaryRoadNumber1 = CustomerInputRules.Required(secondaryRoadNumber1, "El primer número de vía secundaria");
        address.SecondaryRoadNumber2 = CustomerInputRules.Required(secondaryRoadNumber2, "El segundo número de vía secundaria");
        address.MainRoadLetter = CustomerInputRules.Optional(mainRoadLetter, "La letra de vía principal");
        address.MainRoadCardinality = CustomerInputRules.Optional(mainRoadCardinality, "La cardinalidad principal");
        address.SecondaryRoadLetter = CustomerInputRules.Optional(secondaryRoadLetter, "La letra de vía secundaria");
        address.SecondaryRoadCardinality1 = CustomerInputRules.Optional(secondaryRoadCardinality1, "La cardinalidad secundaria 1");
        address.SecondaryRoadCardinality2 = CustomerInputRules.Optional(secondaryRoadCardinality2, "La cardinalidad secundaria 2");
        address.FormattedAddress = string.Join(' ', new[]
        {
            address.MainRoadType, address.MainRoadNumber + address.MainRoadLetter, address.MainRoadCardinality,
            "#", address.SecondaryRoadNumber1 + address.SecondaryRoadLetter, address.SecondaryRoadCardinality1,
            "-", address.SecondaryRoadNumber2, address.SecondaryRoadCardinality2
        }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return address;
    }
}
