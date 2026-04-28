using System.Text.Json;

namespace SimpleMcpServer.Api.Transports
{
    public class StdioServer
    {
        public void Start()
        {
            string? line;

            while ((line = Console.ReadLine()) != null)
            {
                var response = new
                {
                    jsonrpc = "2.0",
                    result = $"echo: {line}",
                    id = 1
                };

                Console.WriteLine(JsonSerializer.Serialize(response));
            }
        }
    }
}
