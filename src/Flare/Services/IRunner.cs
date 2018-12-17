


using System;
using Microsoft.Extensions.Hosting;

namespace Flare.Services
{
    public interface IRunner : IHostedService, IDisposable
     {
        bool IsRunning { get; }
        string Output { get; }
    }
}