using System.Text.RegularExpressions;

namespace DataGo.Domain;

public static partial class NitValidator
{
    private static readonly int[] Weights = [41, 37, 29, 23, 19, 17, 13, 7, 3];

    public static bool IsValid(string documentNumber, string? verificationDigit)
    {
        if (!NineDigits().IsMatch(documentNumber) || verificationDigit is null ||
            verificationDigit.Length != 1 || !char.IsDigit(verificationDigit[0])) return false;

        var sum = documentNumber.Select((digit, index) => (digit - '0') * Weights[index]).Sum();
        var remainder = sum % 11;
        var expected = remainder is 0 or 1 ? remainder : 11 - remainder;
        return expected == verificationDigit[0] - '0';
    }

    [GeneratedRegex("^[0-9]{9}$")]
    private static partial Regex NineDigits();
}

public sealed record ParsedName(string FullName, string FirstNames, string LastNames);

public static class NameParser
{
    // Particles attach to the next word; with three or more components the last two
    // are surnames. Ambiguous three-component names remain editable after creation.
    private static readonly HashSet<string> Particles = new(StringComparer.OrdinalIgnoreCase)
        { "de", "del", "la", "las", "los" };
    public static ParsedName Parse(string legalName)
    {
        var tokens = Regex.Split(legalName.Trim(), @"\s+").Where(x => x.Length > 0).ToArray();
        if (tokens.Length == 0) throw new DomainValidationException("El nombre legal extendido es obligatorio");

        var components = new List<string>();
        var pending = new List<string>();
        foreach (var token in tokens)
        {
            pending.Add(token);
            if (Particles.Contains(token)) continue;
            components.Add(string.Join(' ', pending));
            pending.Clear();
        }
        if (pending.Count > 0)
        {
            if (components.Count == 0) components.Add(string.Join(' ', pending));
            else components[^1] += " " + string.Join(' ', pending);
        }
        var firstCount = components.Count <= 2 ? 1 : components.Count - 2;
        var first = components.Take(firstCount);
        var last = components.Skip(firstCount);
        return new(string.Join(' ', tokens), string.Join(", ", first), string.Join(", ", last));
    }
}

internal static class CustomerInputRules
{
    private static readonly HashSet<string> DummyValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "test", "prueba", "dummy", "n/a", "na", "no aplica", "sin dato", "sin información",
        "000000", "0000000", "1111111", "1234567", "1234567890", "correo@correo.com", "test@test.com"
    };

    public static string Required(string? value, string field)
    {
        var normalized = Normalize(value);
        if (normalized.Length == 0) throw new DomainValidationException($"{field} es obligatorio");
        if (DummyValues.Contains(normalized)) throw new DomainValidationException($"{field} contiene un valor no permitido");
        return normalized;
    }

    public static string? Optional(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = Normalize(value);
        if (DummyValues.Contains(normalized)) throw new DomainValidationException($"{field} contiene un valor no permitido");
        return normalized;
    }

    public static string NameComponents(string? value, string field, bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required) throw new DomainValidationException($"{field} es obligatorio");
            return "";
        }

        var components = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (components.Length == 0 && required) throw new DomainValidationException($"{field} es obligatorio");
        return string.Join(", ", components.Select(component => Required(component, field)));
    }

    public static string JoinNameComponents(params string[] values) => string.Join(' ', values
        .SelectMany(value => value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)));

    private static string Normalize(string? value) => Regex.Replace(value?.Trim() ?? "", @"\s+", " ");
}
