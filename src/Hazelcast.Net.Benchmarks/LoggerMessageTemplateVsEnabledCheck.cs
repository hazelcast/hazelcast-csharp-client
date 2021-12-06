using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Benchmarks
{

    //|                    Method |      _level |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
    //|-------------------------- |------------ |----------:|----------:|----------:|-------:|----------:|
    //| LogWithEnabledWithSiCheck |       Debug |  3.198 ns | 0.0650 ns | 0.0608 ns |      - |         - |
    //|   LogWithMsgTemplateCheck |       Debug | 49.542 ns | 1.0134 ns | 1.0407 ns | 0.0038 |      32 B |
    //| LogWithEnabledWithSiCheck | Information |  3.228 ns | 0.0407 ns | 0.0381 ns |      - |         - |
    //|   LogWithMsgTemplateCheck | Information | 51.612 ns | 1.0422 ns | 0.9749 ns | 0.0038 |      32 B |

    public class LoggerMessageTemplateVsEnabledCheck
    {
        private ILogger _logger;

        [Params(LogLevel.Debug, LogLevel.Information)]
        public LogLevel _level;

        [GlobalSetup]
        public void Setup() { _logger = LoggerFactory.Create(opt => opt.SetMinimumLevel(_level)).CreateLogger("dummy"); }

        //lets keep it short-> Si = String Interpolation

        [Benchmark]
        public void LogWithEnabledWithCheck()
        {
            string msg = "we are sailing to Key Largo, goodluck";

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("something went wrong {msg}", msg);
        }


        [Benchmark]
        public void LogWithMsgTemplateCheck()
        {
            string msg = "we are sailing to Key Largo, goodluck";

            _logger.LogDebug("something went wrong {msg}", msg);
        }


    }
}
