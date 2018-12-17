using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Flare.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Flare.Controllers
{
    [Produces("application/json")]
    public class FlareController : Controller
    {
        private readonly ILogger<FlareController> _logger;
        public readonly IRunner _runner;

        public FlareController(ILogger<FlareController> logger, IRunner processRunnerService)
        {
            _logger = logger;
            _runner = processRunnerService;
        }


        [HttpGet]
        public async Task<string> Output()
        {
            _logger.LogDebug("Output");
            return await Task.Run<string> ( () =>
            {
                return _runner.Output;
            });
        }

        [HttpGet]
        public async Task<bool> IsRunning()
        {
            return await Task.Run<bool>( () => 
                {
                    return _runner.IsRunning;
                });
        }

        [HttpPost]
        public async Task<bool> Start()
        {
            return await Task.Run<bool> (async () =>
            {
                try
                {
                    var tokenSrc = new CancellationTokenSource();
                    await _runner.StartAsync(tokenSrc.Token);
                    return true;
                }
                catch(Exception e)
                {
                    _logger.LogError(e, "Error in starting runner");
                    return false;
                }
            });
        }
        
        [HttpPost]
        public async Task<bool> Stop()
        {
            return await Task.Run<bool> (async () =>
            {
                try
                {
                    var tokenSrc = new CancellationTokenSource();
                    await _runner.StopAsync(tokenSrc.Token);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in stopping runner");
                    return false;
                }
            });
        }

        [HttpPost]
        public async Task<bool> Restart()
        {
            return await Task.Run<bool> (async () =>
            {
                try
                {
                    var tokenSrc = new CancellationTokenSource();
                    await _runner.StopAsync(tokenSrc.Token);
               
                    tokenSrc = new CancellationTokenSource();
                    await _runner.StartAsync(tokenSrc.Token);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in restarting runner");
                    return false;
                }
            });
        }
    }
}