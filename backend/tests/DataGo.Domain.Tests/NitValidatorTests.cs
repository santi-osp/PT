using DataGo.Domain;

namespace DataGo.Domain.Tests;

public sealed class NitValidatorTests
{
    [Theory]
    [InlineData("900123456")]
    [InlineData("901234567")]
    public void Accepts_computed_verification_digit(string number)
    {
        var weights = new[] { 41, 37, 29, 23, 19, 17, 13, 7, 3 };
        var remainder = number.Select((digit, index) => (digit - '0') * weights[index]).Sum() % 11;
        var digit = remainder is 0 or 1 ? remainder : 11 - remainder;

        Assert.True(NitValidator.IsValid(number, digit.ToString()));
    }

    [Theory]
    [InlineData("900123456", "9")]
    [InlineData("123", "1")]
    [InlineData("900123456", null)]
    public void Rejects_invalid_nit(string number, string? digit) =>
        Assert.False(NitValidator.IsValid(number, digit));
}
