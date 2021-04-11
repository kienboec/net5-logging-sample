using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace net5_logging_sample
{
    class Program
    {
        #region MainService

        class MainService : IHostedService
        {
            private readonly ILogger<MainService> _logger;
            private readonly HelperService _helperService;
            private Task _mainTask = null;
            private CancellationTokenSource _cts = null;

            public MainService(ILogger<MainService> logger, HelperService helperService)
            {
                _logger = logger;
                _helperService = helperService;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogDebug("starting");
                _cts = new CancellationTokenSource();
                _mainTask = Task.Run(Execute, _cts.Token);

                await Task.CompletedTask;
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                _cts.Cancel(true);
                _logger.LogInformation("stopped");
                await Task.CompletedTask;
            }

            private async void Execute()
            {
                try
                {
                    while (true)
                    {
                        _logger.LogTrace("log trace");
                        _logger.LogDebug("log debug");
                        _logger.LogInformation("log info");
                        _logger.LogWarning("log warn");
                        _logger.LogError("log error");
                        _logger.LogCritical("log critical");
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        _helperService.DoWork();
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                }
                catch (Exception exc)
                {
                    Debugger.Break();
                }
            }
        }

        #endregion

        #region HelperService

        class HelperService
        {
            private readonly ILogger<HelperService> _logger;

            public HelperService(ILogger<HelperService> logger)
            {
                _logger = logger;
            }

            public void DoWork()
            {
                _logger.LogInformation("Did work!");
            }
        }

        #endregion

        static void Main(string[] args)
        {
            // nuget: Microsoft.Extensions.Hosting
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, builder) =>
                {
                    // here we configure different frameworks or stuff
                    builder.ClearProviders();
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Trace);

                }).ConfigureServices((context, services) =>
                {
                    #region display Configuration 

                    Console.WriteLine("".PadLeft(60, '-'));
                    Console.WriteLine("Configuration:");
                    context.Configuration
                        .AsEnumerable()
                        .OrderBy(x => x.Key)
                        .ToList()
                        .ForEach(x => Console.WriteLine("|" + x.Key + "|" + x.Value + "|"));
                    Console.WriteLine("".PadLeft(60, '-'));
                    Console.WriteLine("Current directory:" + Environment.CurrentDirectory);
                    Console.WriteLine("".PadLeft(60, '-'));
                    Console.WriteLine("Environment: " + context.HostingEnvironment.EnvironmentName);
                    Console.WriteLine("".PadLeft(60, '-'));
                    Console.WriteLine();
                    Console.WriteLine();

                    #endregion

                    services.AddSingleton<HelperService>();
                    services.AddHostedService<MainService>();
                })
                .UseConsoleLifetime()
                .Build()
                .Run();
        }
    }
}
