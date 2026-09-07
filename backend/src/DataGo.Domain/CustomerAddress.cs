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
        var address = new CustomerAddress { Id = Guid.NewGuid(), CustomerId = customerId };
        address.Update(neighborhoodId, isRural, ruralAddress, mainRoadType, mainRoadNumber, mainRoadLetter,
            mainRoadCardinality, secondaryRoadNumber1, secondaryRoadLetter, secondaryRoadCardinality1,
            secondaryRoadNumber2, secondaryRoadCardinality2);
        return address;
    }

    public void Update(Guid neighborhoodId, bool isRural, string? ruralAddress,
        string? mainRoadType, string? mainRoadNumber, string? mainRoadLetter, string? mainRoadCardinality,
        string? secondaryRoadNumber1, string? secondaryRoadLetter, string? secondaryRoadCardinality1,
        string? secondaryRoadNumber2, string? secondaryRoadCardinality2)
    {
        if (neighborhoodId == Guid.Empty) throw new DomainValidationException("El barrio es obligatorio");
        NeighborhoodId = neighborhoodId;
        IsRural = isRural;
        if (isRural)
        {
            RuralAddress = CustomerInputRules.Required(ruralAddress, "La dirección rural");
            MainRoadType = MainRoadNumber = MainRoadLetter = MainRoadCardinality = null;
            SecondaryRoadNumber1 = SecondaryRoadLetter = SecondaryRoadCardinality1 = null;
            SecondaryRoadNumber2 = SecondaryRoadCardinality2 = null;
            FormattedAddress = RuralAddress;
            return;
        }

        RuralAddress = null;
        MainRoadType = CustomerInputRules.Required(mainRoadType, "El tipo de vía principal");
        MainRoadNumber = CustomerInputRules.Required(mainRoadNumber, "El número de vía principal");
        SecondaryRoadNumber1 = CustomerInputRules.Required(secondaryRoadNumber1, "El primer número de vía secundaria");
        SecondaryRoadNumber2 = CustomerInputRules.Required(secondaryRoadNumber2, "El segundo número de vía secundaria");
        MainRoadLetter = CustomerInputRules.Optional(mainRoadLetter, "La letra de vía principal");
        MainRoadCardinality = CustomerInputRules.Optional(mainRoadCardinality, "La cardinalidad principal");
        SecondaryRoadLetter = CustomerInputRules.Optional(secondaryRoadLetter, "La letra de vía secundaria");
        SecondaryRoadCardinality1 = CustomerInputRules.Optional(secondaryRoadCardinality1, "La cardinalidad secundaria 1");
        SecondaryRoadCardinality2 = CustomerInputRules.Optional(secondaryRoadCardinality2, "La cardinalidad secundaria 2");
        FormattedAddress = string.Join(' ', new[]
        {
            MainRoadType, MainRoadNumber + MainRoadLetter, MainRoadCardinality,
            "#", SecondaryRoadNumber1 + SecondaryRoadLetter, SecondaryRoadCardinality1,
            "-", SecondaryRoadNumber2, SecondaryRoadCardinality2
        }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
