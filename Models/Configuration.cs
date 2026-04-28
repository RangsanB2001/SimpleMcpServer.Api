namespace SimpleMcpServer.Api.Models
{
    public class McpServerOptions
    {
        public const string SectionName = "McpServer";

        public string Name { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public bool EnableWebSocket { get; set; }

        public bool EnableStdio { get; set; }
    }
    public class AuthOptions
    {
        public const string SectionName = "Auth";
        public string ApiKey { get; set; } = string.Empty;
    }
    public class RedisOptions
    {
        public const string SectionName = "Redis";

        public string ConnectionString { get; set; } = string.Empty;
    }
}
