
using System;
using System.Diagnostics;
using System.Timers;
using System.Threading.Tasks;
using Flare.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Flare.Services
{
    public class PollRunner : IRunner
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
        private System.Timers.Timer _timer;

        public bool IsRunning => _timer != null ? _timer.Enabled : false;

        public string Output { get; private set; }

        public PollRunner(ILogger<PollRunner> logger, IConfiguration configuration, IHubContext<FlareHub> hubContext)
        {
            _logger = logger;
            _configuration = configuration;
            _hubContext = hubContext;
            Output = string.Empty;
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
            _timer = new System.Timers.Timer();
            _timer.Elapsed += timer_callback;
            _timer.Interval = TimeSpan.FromSeconds(_configuration.GetValue<double>(PERIOD_SECONDS_KEY, DEFAULT_PERIOD_SECONDS)).TotalMilliseconds;
            _timer.Start();
            _logger.LogDebug("Process Runner Started.");
        }

        private async void timer_callback(object sener, EventArgs e)
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
                        Output = string.Empty;
                        Output += output;
                        Output += error;
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
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error");
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
            _timer?.Stop();
            _logger.LogDebug("Process Runner Stopped.");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }   
    }
}