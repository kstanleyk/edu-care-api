namespace EduCare.Domain.ValueObjects;

public record Currency
{
    public string Code { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public int DecimalPlaces { get; init; }

    // EF Core requires a parameterless constructor
    protected Currency() { }

    public Currency(string code, string symbol, int decimalPlaces)
    {
        Code = code;
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    public static readonly Currency XOF = new("XOF", "CFA", 2);

    public static Currency FromCode(string code)
    {
        return code.ToUpper() switch
        {
            "XOF" => XOF,
            _ => throw new ArgumentException($"Unsupported currency code: {code}")
        };
    }

    public static IReadOnlyList<Currency> All => [XOF];
}