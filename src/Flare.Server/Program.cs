

using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Flare.Server;

public class Program
{
    private static List<WebSocket> s_webSockets = new List<WebSocket>();
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.UseWebSockets();

        if (Debugger.IsAttached)
            app.MapGet("/", () => "Hello World!");

        app.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    Console.WriteLine($"Adding websocket to collection: {s_webSockets.Count}");
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    s_webSockets.Add(webSocket);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            });

        Thread background = new Thread(async () =>
        {
            char[] buffer = new char[1024];
            int bytesRead = await Console.In.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.ForEach(s_webSockets, async (WebSocket s) =>
                {
                    await s.SendAsync(
                        buffer: Encoding.UTF8.GetBytes(buffer),
                        messageType: WebSocketMessageType.Text,
                        endOfMessage: true,
                        cancellationToken: CancellationToken.None
                    );
                });
            }
            Thread.Sleep(100);
        });
        background.Start();

        app.Run();

    }
}
