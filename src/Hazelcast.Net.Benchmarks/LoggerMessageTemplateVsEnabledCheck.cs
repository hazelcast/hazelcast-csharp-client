using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Benchmarks
{

    //|                  Method |      _level |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
    //|------------------------ |------------ |----------:|----------:|----------:|-------:|----------:|
    //| LogWithEnabledWithCheck |       Debug |  3.419 ns | 0.0159 ns | 0.0124 ns |      - |         - |
    //| LogWithMsgTemplateCheck |       Debug | 49.457 ns | 0.2508 ns | 0.2346 ns | 0.0038 |      32 B |
    //| LogWithEnabledWithCheck | Information |  3.455 ns | 0.0182 ns | 0.0161 ns |      - |         - |
    //| LogWithMsgTemplateCheck | Information | 48.586 ns | 0.3233 ns | 0.3024 ns | 0.0038 |      32 B |

    public class LoggerMessageTemplateVsEnabledCheck
    {
        private ILogger _logger;

        [Params(LogLevel.Debug, LogLevel.Information)]
        public LogLevel _level;

        [GlobalSetup]
        public void Setup() { _logger = LoggerFactory.Create(opt => opt.SetMinimumLevel(_level)).CreateLogger("dummy"); }

        readonly string msg = "we are sailing to Key Largo, goodluck";

        [Benchmark]
        public void LogWithEnabledCheck()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("something went wrong {msg}", msg);
        }

        [Benchmark]
        public void LogWithMsgTemplateCheck()
        {
            _logger.LogDebug("something went wrong {msg}", msg);
        }
    }
}
