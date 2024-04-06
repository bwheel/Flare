

namespace Flare;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.UseWebSockets();
        app.MapGet("/", () => "Hello World!");
        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
                {
                    while (true)
                    {
                        await webSocket.SendAsync(Encoding.ASCII.GetBytes($"Test - {DateTime.Now}"), WebSocketMessageType.Text, true, CancellationToken.None);
                        await Task.Delay(1000);
                    }
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        });

        app.Run();
    }
}
