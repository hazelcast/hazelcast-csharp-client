// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Implements a <see cref="IRetryStrategy"/> with back-off and timeout.
    /// </summary>
    internal class RetryStrategy : IRetryStrategy
    {
        private readonly string _action;
        private readonly ILogger _logger;

        private readonly int _initialBackoffMilliseconds;
        private readonly int _maxBackoffMilliseconds;
        private readonly double _multiplier;
        private readonly long _timeoutMilliseconds;
        private readonly double _jitter;
        private int _currentBackOffMilliseconds;
        private int _attempts;
        private DateTime _begin;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryStrategy"/> class.
        /// </summary>
        /// <param name="action">The description of the action.</param>
        /// <param name="options">Configuration.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public RetryStrategy(string action, ConnectionRetryOptions options, ILoggerFactory loggerFactory)
            : this(action,
                (options ?? throw new ArgumentNullException(nameof(options))).InitialBackoffMilliseconds,
                options.MaxBackoffMilliseconds,
                options.Multiplier,
                options.ClusterConnectionTimeoutMilliseconds,
                options.Jitter,
                loggerFactory)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryStrategy"/> class.
        /// </summary>
        /// <param name="action">The description of the action.</param>
        /// <param name="initialBackOffMilliseconds">The initial back-off value in milliseconds.</param>
        /// <param name="maxBackOffMilliseconds">The maximum back-off value in milliseconds.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        /// <param name="jitter">A jitter factor.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public RetryStrategy(string action, int initialBackOffMilliseconds, int maxBackOffMilliseconds, double multiplier, long timeoutMilliseconds, double jitter, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(action));
#pragma warning disable CA1308 // Normalize strings to uppercase - not normalizing, just lower-casing for display
            _action = action.ToLowerInvariant();
#pragma warning restore CA1308

            if (initialBackOffMilliseconds < 0) throw new ConfigurationException("Initial back-off must be greater than or equal to zero.");
            _initialBackoffMilliseconds = initialBackOffMilliseconds;
            if (maxBackOffMilliseconds < 0) throw new ConfigurationException("Maximum back-off must be greater than or equal to zero.");
            _maxBackoffMilliseconds = maxBackOffMilliseconds;
            if (multiplier <= 0) throw new ConfigurationException("Multiplier must be greater than zero.");
            _multiplier = multiplier;
            _timeoutMilliseconds = timeoutMilliseconds;
            if (jitter < 0 || jitter > 1) throw new ConfigurationException("Jitter must be between zero and one, inclusive.");
            _jitter = jitter;

            _logger = loggerFactory?.CreateLogger<RetryStrategy>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            Restart();
        }

        /// <summary>
        /// (internal for tests only) Gets the delay.
        /// </summary>
        internal int GetDelay(int elapsed)
        {
            // java:
            // long actualSleepTime = (long) (currentBackoffMillis +currentBackoffMillis * jitter * (2.0 * random.nextDouble() - 1.0));
            //
            // delay is _currentBackOffMilliseconds + _currentBackOffMilliseconds * jitter * random
            // where random is between -1 and +1 and _jitter is between 0 and 1

            var rand = 2.0 * RandomProvider.NextDouble() - 1.0; // -1 to +1
            var delay = (int)(_currentBackOffMilliseconds * (1 + _jitter * rand));
            if (_timeoutMilliseconds >= 0) delay = Math.Min(delay, Math.Max(0, (int)(_timeoutMilliseconds - elapsed)));
            return delay;
        }

        /// <summary>
        /// (internal for tests only) Gets the new back-off.
        /// </summary>
        internal int GetNewBackoff()
        {
            return (int)Math.Min(_currentBackOffMilliseconds * _multiplier, _maxBackoffMilliseconds);
        }

        /// <inheritdoc />
        public async ValueTask<bool> WaitAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            _attempts++;

            var elapsed = (int)(DateTime.UtcNow - _begin).TotalMilliseconds;

            _logger.IfDebug()?.LogDebug("Elapsed {Elapsed}ms, timeout={TimeoutMilliseconds}ms, back-off={CurrentBackOffMilliseconds}ms ({InitialBackOffMilliseconds}-{MaxBackOffMilliseconds}, m={Multiplier}, j={Jitter})", elapsed, _timeoutMilliseconds, _currentBackOffMilliseconds, _initialBackoffMilliseconds, _maxBackoffMilliseconds, _multiplier, _jitter);

            if (_timeoutMilliseconds > 0 && elapsed > _timeoutMilliseconds)
            {
                _logger.IfWarning()?.LogWarning("Unable to {Action} after {Attempts} attempts and {Elapsed}ms, timeout ({TimeoutMilliseconds}ms).", _action, _attempts, elapsed, _timeoutMilliseconds);
                return false;
            }

            var delay = GetDelay(elapsed);

            _logger.IfWarning()?.LogWarning("Unable to {Action} after {Attempts} attempts and {Elapsed}ms, will retry in {Delay}ms", _action, _attempts, elapsed, delay);

            try
            {
                await Task.Delay(delay, cancellationToken).CfAwait();
            }
            catch (OperationCanceledException)
            {
                _logger.IfWarning()?.LogWarning("Unable to {Action} after {Attempts} attempts and {Elapsed}ms, cancelled.", _action, _attempts, elapsed);
                return false;
            }
            catch (Exception)
            {
                _logger.IfWarning()?.LogWarning("Unable to {Action} after {Attempts} attempts and {Elapsed}ms, error.", _action, _attempts, elapsed);
                return false;
            }

            _currentBackOffMilliseconds = GetNewBackoff();
            return true;
        }

        /// <inheritdoc />
        public void Restart()
        {
            _attempts = 0;
            _currentBackOffMilliseconds = Math.Min(_maxBackoffMilliseconds, _initialBackoffMilliseconds);
            _begin = DateTime.UtcNow;
        }
    }
}
