using System.Net.WebSockets;
using System.Text;

namespace SimpleMcpServer.Api.Transports
{
    public static class WebSocketHandler
    {
        public static async Task HandleAsync(HttpContext context, WebSocket socket)
        {
            var buffer = new byte[4096];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var response = Encoding.UTF8.GetBytes($"echo: {text}");

                await socket.SendAsync(response, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
