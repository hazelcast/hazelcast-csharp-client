// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
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
