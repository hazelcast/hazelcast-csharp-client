// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System;
using System.Threading;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Network
{
    internal class WaitStrategy
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(WaitStrategy));

        private readonly int _initialBackOffMillis;
        private readonly int _maxBackOffMillis;
        private readonly double _multiplier;
        private readonly double _jitter;
        private readonly long _clusterConnectTimeoutMillis;
        private readonly Random _random = new Random();

        private int _attempt;
        private int _currentBackOffMillis;
        private long _clusterConnectAttemptBegin;

        public WaitStrategy(int initialBackOffMillis, int maxBackOffMillis, double multiplier, long clusterConnectTimeoutMillis,
            double jitter)
        {
            _initialBackOffMillis = initialBackOffMillis;
            _maxBackOffMillis = maxBackOffMillis;
            _multiplier = multiplier;
            _clusterConnectTimeoutMillis = clusterConnectTimeoutMillis;
            _jitter = jitter;
        }

        public void Reset()
        {
            _attempt = 0;
            _clusterConnectAttemptBegin = Clock.CurrentTimeMillis();
            _currentBackOffMillis = Math.Min(_maxBackOffMillis, _initialBackOffMillis);
        }

        public bool Sleep()
        {
            _attempt++;
            var currentTimeMillis = Clock.CurrentTimeMillis();
            var timePassed = currentTimeMillis - _clusterConnectAttemptBegin;
            if (timePassed > _clusterConnectTimeoutMillis)
            {
                Logger.Warning(
                    $"Unable to get live cluster connection, cluster connect timeout ({_clusterConnectTimeoutMillis} millis) is reached. Attempt {_attempt}.");
                return false;
            }

            //random_between
            // Random(-jitter * current_backOff, jitter * current_backOff)
            long actualSleepTime = (long) (_currentBackOffMillis - (_currentBackOffMillis * _jitter) +
                                           (_currentBackOffMillis * _jitter * _random.NextDouble()));

            actualSleepTime = Math.Min(actualSleepTime, _clusterConnectTimeoutMillis - timePassed);

            Logger.Warning(
                $"Unable to get live cluster connection, retry in {actualSleepTime} ms, attempt: {_attempt}, cluster connect timeout: {_clusterConnectTimeoutMillis} seconds , max backOff millis: {_maxBackOffMillis}");

            try
            {
                Thread.Sleep((int) actualSleepTime);
            }
            catch (ThreadInterruptedException)
            {
                return false;
            }

            _currentBackOffMillis = (int) Math.Min(_currentBackOffMillis * _multiplier, _maxBackOffMillis);
            return true;
        }
    }
}