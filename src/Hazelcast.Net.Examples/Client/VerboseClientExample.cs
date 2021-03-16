using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Hazelcast.Core;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// this is an example of debugging a client by enabling verbose logging & output
// (was initially created to troubleshoot duplicate connections to docker servers)
//
// docker: docker run -p 5701:5701 hazelcast/hazelcast:latest-snapshot
// build: hz -d HZ_CONSOLE,HZ_CONSOLE_PUBLIC -c Debug build
// run: src/Hazelcast.Net.Examples/bin/Debug/netcoreapp3.1/hx VerboseClient

#if HZ_CONSOLE_PUBLIC

namespace Hazelcast.Examples
{
    public static class VerboseClientExample
    {
        public static async Task Main(string[] args)
        {
            var h = new object();
            using var _ = HConsole.Capture(options => options
                .ClearAll()
                .Set(x => x.SetLevel(1))
                .Set(h, x => x.SetPrefix("PROGRAM"))
                .Set<AsyncContext>(x => x.Quiet())
                .Set("Hazelcast.Networking.SocketConnectionBase", x => x.SetIndent(1).SetLevel(0).SetPrefix("SOCKET"))
                .Set("Hazelcast.Clustering.MemberConnection", x => x.SetLevel(1))
            );

            var options = new HazelcastOptionsBuilder()
                .With(args)
                .With("Logging:LogLevel:Hazelcast", "Debug")
                .WithHConsoleLogger()
                .Build();

            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("127.0.0.1:5701");
            options.ClusterName = "dev";

            var logger = options.LoggerFactory.Service.CreateLogger<Program>();
            logger.LogInformation("Begin.");

            logger.LogInformation("Start client...");
            var hz = await HazelcastClientFactory.StartNewClientAsync(options);

            logger.LogWarning("Wait...");
            await Task.Delay(1000);

            logger.LogWarning("Dispose client...");
            await hz.DisposeAsync();

            logger.LogWarning("End.");
            options.LoggerFactory.Service.Dispose();
        }

        // configure logging with an HConsole logger (internal for troubleshooting purposes)
        private static HazelcastOptionsBuilder WithHConsoleLogger(this HazelcastOptionsBuilder builder)
        {
            return builder
                .With("Logging:LogLevel:Default", "Debug")
                .With("Logging:LogLevel:System", "Information")
                .With("Logging:LogLevel:System", "Information")
                .With((configuration, options) =>
                {
                    // configure logging factory and add the console provider
                    options.LoggerFactory.Creator = () => LoggerFactory.Create(loggingBuilder =>
                        loggingBuilder
                            .AddConfiguration(configuration.GetSection("logging"))
                            .AddHConsole());
                });
        }
        
        // adds an HConsole logger (internal for troubleshooting purposes)
        private static ILoggingBuilder AddHConsole(this ILoggingBuilder builder, TestingLoggerOptions options = null)
        {
            builder.AddConfiguration();

            var descriptor = ServiceDescriptor.Singleton<ILoggerProvider>(new HConsoleLoggerProvider(options));
            builder.Services.TryAddEnumerable(descriptor);
            return builder;
        }
    }
}

#endif