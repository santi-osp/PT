namespace DataGo.Domain;

public sealed class Country
{
    private Country() { }
    public Country(Guid id, string name) => (Id, Name) = (id, name);
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
}

public sealed class Department
{
    private Department() { }
    public Department(Guid id, Guid countryId, string name) => (Id, CountryId, Name) = (id, countryId, name);
    public Guid Id { get; private set; }
    public Guid CountryId { get; private set; }
    public string Name { get; private set; } = "";
}

public sealed class Municipality
{
    private Municipality() { }
    public Municipality(Guid id, Guid departmentId, string name) => (Id, DepartmentId, Name) = (id, departmentId, name);
    public Guid Id { get; private set; }
    public Guid DepartmentId { get; private set; }
    public string Name { get; private set; } = "";
}

public sealed class TransportZone
{
    private TransportZone() { }
    public TransportZone(Guid id, string code) => (Id, Code) = (id, code);
    public Guid Id { get; private set; }
    public string Code { get; private set; } = "";
}

public sealed class Neighborhood
{
    private Neighborhood() { }
    public Neighborhood(Guid id, Guid municipalityId, Guid transportZoneId, string name) =>
        (Id, MunicipalityId, TransportZoneId, Name) = (id, municipalityId, transportZoneId, name);
    public Guid Id { get; private set; }
    public Guid MunicipalityId { get; private set; }
    public Guid TransportZoneId { get; private set; }
    public string Name { get; private set; } = "";
}

public sealed class Center
{
    private Center() { }
    public Center(Guid id, string code, string name, bool isActive = true) => (Id, Code, Name, IsActive) = (id, code, name, isActive);
    public Guid Id { get; private set; }
    public string Code { get; private set; } = "";
    public string Name { get; private set; } = "";
    public bool IsActive { get; private set; }
}

public sealed class ModernChannelCustomer
{
    private ModernChannelCustomer() { }
    public ModernChannelCustomer(Guid id, DocumentType documentType, string documentNumber) =>
        (Id, DocumentType, DocumentNumber) = (id, documentType, documentNumber);
    public Guid Id { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; } = "";
}
