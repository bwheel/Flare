

using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Flare.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.UseWebSockets();
        app.MapGet("/", () => "Hello World!");

        using (var stdin = Console.OpenStandardInput())
        {
            app.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        while (true)
                        {
                            byte[] buffer = new byte[1024];
                            await stdin.ReadAsync(buffer, 0, buffer.Length);                            
                            await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                            await Task.Delay(1000);
                        }
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            });
        }

        app.Run();
    }
}
