namespace SimpleMcpServer.Api.Mcp.Tools
{
    public class CustomerLookupTool : IDataTool
    {
        public string Name => "customer_lookup";
        public string Description => "Lookup customer information by customer id";

        public object InputSchema => new
        {
            type = "object",
            properties = new
            {
                customer_id = new { type = "integer" }
            }
        };

        public Task<object> ExecuteAsync(
            Dictionary<string, object>? arguments,
            CancellationToken cancellationToken = default)
        {
            var id = Convert.ToInt32(arguments?["customer_id"]);
            return Task.FromResult<object>(new
            {
                Id = id,
                Name = "Enterprise Customer",
                Tier = "Gold"
            });
        }
    }
}
