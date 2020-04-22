// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Hazelcast.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a retry strategy.
    /// </summary>
    /// <remarks>
    /// <para>Controls retries with a back-off mechanism.</para>
    /// </remarks>
    public class RetryStrategy
    {
        [SuppressMessage("NDepend", "ND3101:DontUseSystemRandomForSecurityPurposes", Justification = "No security here.")]
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);

        private readonly int _initialBackOff;
        private readonly int _maxBackOff;
        private readonly double _multiplier;
        private readonly int _timeout;
        private readonly double _jitter;

        private int _currentBackOff;
        private int _attempts;
        private DateTime _begin;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryStrategy"/> class.
        /// </summary>
        /// <param name="initialBackOff">The initial back-off value in milliseconds.</param>
        /// <param name="maxBackOff">The maximum back-off value in milliseconds.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="jitter">A jitter factor.</param>
        public RetryStrategy(int initialBackOff, int maxBackOff, double multiplier, int timeout, double jitter)
        {
            _initialBackOff = initialBackOff;
            _maxBackOff = maxBackOff;
            _multiplier = multiplier;
            _timeout = timeout;
            _jitter = jitter;

            Restart();
        }

        /// <summary>
        /// Restarts the strategy.
        /// </summary>
        public void Restart()
        {
            _attempts = 0;
            _currentBackOff = Math.Min(_maxBackOff, _initialBackOff);
            _begin = DateTime.UtcNow;
        }

        /// <summary>
        /// Waits before retrying.
        /// </summary>
        /// <returns>Whether it is ok to retry.</returns>
        /// <remarks>
        /// <para>Returns false when the timeout has been reached.</para>
        /// </remarks>
        public async ValueTask<bool> WaitAsync()
        {
            _attempts++;

            var elapsed = (int) (DateTime.UtcNow - _begin).TotalMilliseconds;

            if (elapsed > _timeout)
            {
                XConsole.WriteLine(this, $"Unable to connect to cluster after {_timeout} ms and {_attempts} attempts");
                return false;
            }

            var delay = (int) (_currentBackOff * (1 - _jitter * (1 - Random.NextDouble())));
            delay = Math.Min(delay, _timeout - elapsed);

            XConsole.WriteLine(this, $"Unable to connect to cluster after {_attempts} attempts and {elapsed} ms, retrying in {delay} ms");

            try
            {
                await Task.Delay(delay);
            }
            catch (Exception)
            {
                return false;
            }

            _currentBackOff = (int) Math.Min(_currentBackOff * _multiplier, _maxBackOff);
            return true;
        }
    }
}