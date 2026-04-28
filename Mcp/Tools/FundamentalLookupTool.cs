using SimpleMcpServer.Api.Models;
using SimpleMcpServer.Api.Providers;

namespace SimpleMcpServer.Api.Mcp.Tools
{
    public class FundamentalLookupTool : IDataTool
    {
        private readonly IDataProvider<FundamentalData> _provider;

        public FundamentalLookupTool(IDataProvider<FundamentalData> provider)
        {
            _provider = provider;
        }

        public string Name => "fundamental_lookup";
        public string Description => "Get stock fundamental financial data by symbol";
        public object InputSchema => new
        {
            type = "object",
            properties = new
            {
                symbol = new
                {
                    type = "string",
                    description = "Stock symbol"
                }
            },
            required = new[] { "symbol" }
        };

        public async Task<object> ExecuteAsync(Dictionary<string, object>? arguments, CancellationToken cancellationToken = default)
        {
            var symbol = arguments?["symbol"]?.ToString();

            if (string.IsNullOrWhiteSpace(symbol))
            {
                return new
                {
                    error = "symbol is required"
                };
            }

            var result = await _provider.GetByKeyAsync(symbol, cancellationToken);

            if (result == null)
            {
                var msg = new
                {
                    msg = "no data"
                };
                return msg;
            }

            return result;
        }
    }
}
