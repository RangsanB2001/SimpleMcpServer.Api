using SimpleMcpServer.Api.Models;

namespace SimpleMcpServer.Api.Providers
{
    public class FundamentalProvider : IDataProvider<FundamentalData>
    {
        private static readonly List<FundamentalData>
            _mockDb =
            [
                new FundamentalData
                        {
                            Symbol = "AAPL",
                            Revenue = 394328000000,
                            NetProfit = 99803000000,
                            PE = 29.5m,
                            ROE = 160.3m,
                            EPS = 6.13m,
                            LastUpdated = DateTime.UtcNow
                        },
                        new FundamentalData
                        {
                            Symbol = "MSFT",
                            Revenue = 211915000000,
                            NetProfit = 72361000000,
                            PE = 34.2m,
                            ROE = 38.1m,
                            EPS = 9.68m,
                            LastUpdated = DateTime.UtcNow
                        }
            ];

        public Task<FundamentalData?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            var result = _mockDb.FirstOrDefault(
                x => x.Symbol.Equals(
                    key,
                    StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<IEnumerable<FundamentalData>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<FundamentalData>>(
                _mockDb);
        }
    }
}
