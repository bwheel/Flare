using System;
using System.Diagnostics;
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
        public readonly IProcessRunnerService _processRunnerService;

        public FlareController(ILogger<FlareController> logger, IProcessRunnerService processRunnerService)
        {
            _logger = logger;
            _processRunnerService = processRunnerService;
        }


        [HttpGet]
        public async Task<string> Output()
        {
            _logger.LogDebug("Output");
            return await Task.Run<string> ( () =>{
                return "output of some process/script being run.";
            });
        }

        [HttpGet]
        public async Task<bool> IsRunning()
        {
            return await Task.Run<bool>( () => 
                {
                    return false;
                });
        }

        [HttpPost]
        public async Task<bool> Start()
        {
            return await Task.Run<bool> (() =>
            {
                return false;
            });
        }
        
        [HttpPost]
        public async Task<bool> Stop()
        {
            return await Task.Run<bool> (() =>
            {
                return false;
            });
        }

        [HttpPost]
        public async Task<bool> Restart()
        {
            return await Task.Run<bool> (() =>
            {
                return false;
            });
        }
    }
}