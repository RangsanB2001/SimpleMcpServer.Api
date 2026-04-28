using SimpleMcpServer.Api.Transports;

namespace SimpleMcpServer.Api.Extensions
{
    public static class WebSocketEndpointExtensions
    {
        public static void MapWebSocketEndpoints(this WebApplication app)
        {
            app.Map("/ws", async context =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                using var socket =
                    await context.WebSockets.AcceptWebSocketAsync();

                await WebSocketHandler.HandleAsync(
                    context,
                    socket);
            });
        }
    }
}
