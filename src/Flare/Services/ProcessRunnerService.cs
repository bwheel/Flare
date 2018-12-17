
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Flare.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flare.Services
{
    public class ProcessRunnerService : IProcessRunnerService
    {
        // constants.
        public const string PERIOD_SECONDS_KEY = "period";
        private const double DEFAULT_PERIOD_SECONDS = 0.5;

        public const string COMMAND_KEY = "command";
        private const string DEFAULT_COMMAND = "tasklist.exe";

        private const string COMMAND_TIMEOUT_KEY = "timeout";
        private const int DEFAULT_COMMAND_TIMEOUT = 500;

        // services.
        private readonly ILogger _logger;
        public readonly IConfiguration _configuration;
        private readonly IHubContext<FlareHub> _hubContext;

        // members
        private Timer _timer;
    
        public ProcessRunnerService(ILogger<ProcessRunnerService> logger, IConfiguration configuration, IHubContext<FlareHub> hubContext)
        {
            _logger = logger;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register( () => { stop(); });
            start();
            return Task.CompletedTask;
        }

        private void start()
        {
            _logger.LogDebug("Process Runner Starting.");
            _timer = new Timer(
                callback: timer_callback,
                state: null,
                dueTime: TimeSpan.Zero, 
                period: TimeSpan.FromSeconds(_configuration.GetValue<double>(PERIOD_SECONDS_KEY, DEFAULT_PERIOD_SECONDS)));
            _logger.LogDebug("Process Runner Started.");
        }

        private async void timer_callback(object state)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {_configuration.GetValue<string>(COMMAND_KEY, DEFAULT_COMMAND)}",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using(var p = new Process())
                {
                    p.StartInfo = startInfo;
                    if( p.Start() )
                    {
                        p.WaitForExit(_configuration.GetValue<int>(COMMAND_TIMEOUT_KEY, DEFAULT_COMMAND_TIMEOUT));
                        var output = await p.StandardOutput.ReadToEndAsync();
                        var error = await p.StandardError.ReadToEndAsync();
                        var exitCode = p.ExitCode;

                        await _hubContext.Clients.All.SendAsync("output", output);
                        await _hubContext.Clients.All.SendAsync("error", error);
                        await _hubContext.Clients.All.SendAsync("exitCode", exitCode);                        
                    }
                    else
                    {
                        _logger.LogError("error!");
                        await _hubContext.Clients.All.SendAsync("error", startInfo.ToString());
                    }
                }
            }
            catch(Exception e)
            {
                _logger.LogError("Error", e);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register( () => { start(); });
            stop();
            return Task.CompletedTask;
        }

        private void stop()
        {
            _logger.LogDebug("Process Runner Stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogDebug("Process Runner Stopped.");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }   
    }
}