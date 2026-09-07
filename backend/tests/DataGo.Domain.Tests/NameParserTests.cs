using DataGo.Domain;

namespace DataGo.Domain.Tests;

public sealed class NameParserTests
{
    [Theory]
    [InlineData("Ana", "Ana", "")]
    [InlineData("Ana Pérez", "Ana", "Pérez")]
    [InlineData("Ana María Pérez", "Ana, María", "Pérez")]
    [InlineData("Ana María Pérez Gómez", "Ana, María", "Pérez, Gómez")]
    [InlineData("Ana María del Pilar Pérez Gómez", "Ana, María, del, Pilar", "Pérez, Gómez")]
    public void Parses_names_using_documented_assumption(string input, string firstNames, string lastNames)
    {
        var result = NameParser.Parse(input);
        Assert.Equal(input, result.FullName);
        Assert.Equal(firstNames, result.FirstNames);
        Assert.Equal(lastNames, result.LastNames);
    }
}
