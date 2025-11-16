using UptimeKuma.Utils;
using k8s;
using KubeOps.Operator;
using UptimeKuma;
using UptimeKuma.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Console;
using KubeOps.Abstractions.Builder;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class Program
{


    private static async Task Main(string[] args)
    {
        await Console.Out.WriteLineAsync("Starting uptime-kuma-operator");
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.SetMinimumLevel(LogLevel.Trace);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", false, true)
            .Build();

        builder.Services
            .AddSingleton<UptimeKumaManager>()
            .AddSingleton<UptimeKumaService>()
            .AddSingleton<KubernetesService>()
            .AddSingleton<SettingsManager>()
            .AddLogging(logging =>
                logging.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                }))
            .AddKubernetesOperator()
            .RegisterComponents();



        using (var host = builder.Build())
        {
            SocketWrapper.logger = host.Services.GetRequiredService<ILogger<SocketWrapper>>();
            if (host.Services.GetRequiredService<SettingsManager>().ValidateSettings())
            {
                host.Services.GetRequiredService<UptimeKumaManager>().Init();
            }
            await host.RunAsync();
        }
    }
}