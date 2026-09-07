namespace DataGo.Domain;

public sealed class DomainValidationException(string message) : Exception(message);
