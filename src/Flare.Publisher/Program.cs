using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace Flare.Publisher;

class Program
{
    private const string c_hubName = "messageHub";
    static string findServerUrl(string[] args)
    {
        if(args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            return prefixHubName(args[0]);
        string? envVar = Environment.GetEnvironmentVariable("FLARE_PUBLISHER_URL");
        if (!string.IsNullOrWhiteSpace(envVar))
            return prefixHubName(envVar);
        if (Debugger.IsAttached)
            return prefixHubName("https://localhost:7191");
        throw new ArgumentException("Missing Flare Url");
    }

    static string prefixHubName(string baseUrl) => $"{baseUrl.TrimEnd('/')}/{c_hubName}";
    static async Task Main(string[] args)
    {
        string serverUrl = findServerUrl(args);
        var connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .Build();
        try
        {
            // Start the connection
            await connection.StartAsync();

            // Get user input to send to the hub
            while (true)
            {
                var message = Console.In.ReadLine();
                if (string.IsNullOrWhiteSpace(message))
                    continue;

                // Send message to the server
                await connection.InvokeAsync("SendMessage", message);
                Console.Error.WriteLine(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Clean up the connection
            await connection.StopAsync();
            await connection.DisposeAsync();
            Console.WriteLine("Disconnected.");
        }
    }
}
