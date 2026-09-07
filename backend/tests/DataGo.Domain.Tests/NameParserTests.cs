using DataGo.Domain;

namespace DataGo.Domain.Tests;

public sealed class NameParserTests
{
    [Theory]
    [InlineData("Ana", "Ana", "")]
    [InlineData("Ana Pérez", "Ana", "Pérez")]
    [InlineData("Ana María Pérez", "Ana", "María, Pérez")]
    [InlineData("Ana María Pérez Gómez", "Ana, María", "Pérez, Gómez")]
    [InlineData("Ana María del Pilar Pérez Gómez", "Ana, María, del Pilar", "Pérez, Gómez")]
    [InlineData("Juan Pérez", "Juan", "Pérez")]
    [InlineData("Juan Pérez Gómez", "Juan", "Pérez, Gómez")]
    [InlineData("Juan Carlos Pérez", "Juan", "Carlos, Pérez")]
    [InlineData("Juan Carlos Pérez Gómez", "Juan, Carlos", "Pérez, Gómez")]
    [InlineData("María José Pérez Gómez", "María, José", "Pérez, Gómez")]
    [InlineData("María José De la Hoz Pérez", "María, José", "De la Hoz, Pérez")]
    [InlineData("Ana María Pérez-Gómez", "Ana", "María, Pérez-Gómez")]
    [InlineData("José Luis O'Connor Pérez", "José, Luis", "O'Connor, Pérez")]
    public void Parses_names_using_documented_assumption(string input, string firstNames, string lastNames)
    {
        var result = NameParser.Parse(input);
        Assert.Equal(input, result.FullName);
        Assert.Equal(firstNames, result.FirstNames);
        Assert.Equal(lastNames, result.LastNames);
    }

    [Fact]
    public void Normalizes_whitespace_without_changing_accents_or_case()
    {
        var result = NameParser.Parse("  María\tJosé   De la Hoz\nPérez  ");
        Assert.Equal("María José De la Hoz Pérez", result.FullName);
        Assert.Equal("De la Hoz, Pérez", result.LastNames);
    }
}
