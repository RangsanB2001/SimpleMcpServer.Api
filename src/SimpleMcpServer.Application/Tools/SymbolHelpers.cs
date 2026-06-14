namespace SimpleMcpServer.Application.Tools;

internal static class SymbolHelpers
{
    public static string Normalize(string symbol, string parameterName = "symbol")
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException($"{parameterName} must not be empty", parameterName);
        }

        return symbol.Trim().ToUpperInvariant();
    }
}
