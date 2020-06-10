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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents a retry strategy.
    /// </summary>
    /// <remarks>
    /// <para>Controls retries with a back-off mechanism.</para>
    /// </remarks>
    public class RetryStrategy : IRetryStrategy
    {
        private readonly int _initialBackOff;
        private readonly int _maxBackOff;
        private readonly double _multiplier;
        private readonly long _timeout;
        private readonly double _jitter;
        private readonly string _action;
        private readonly ILogger _logger;

        private int _currentBackOff;
        private int _attempts;
        private DateTime _begin;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryStrategy"/> class.
        /// </summary>
        /// <param name="action">The description of the action.</param>
        /// <param name="options">Configuration.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public RetryStrategy(string action, RetryOptions options, ILoggerFactory loggerFactory)
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
        /// <param name="initialBackOff">The initial back-off value in milliseconds.</param>
        /// <param name="maxBackOff">The maximum back-off value in milliseconds.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="jitter">A jitter factor.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public RetryStrategy(string action, int initialBackOff, int maxBackOff, double multiplier, long timeout, double jitter, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(action));
#pragma warning disable CA1308 // Normalize strings to uppercase - not normalizing, just displaying
            _action = action.ToLowerInvariant();
#pragma warning restore CA1308
            _initialBackOff = initialBackOff;
            _maxBackOff = maxBackOff;
            _multiplier = multiplier;
            _timeout = timeout;
            _jitter = jitter;
            _logger = loggerFactory?.CreateLogger<RetryStrategy>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            Restart();
        }

        /// <inheritdoc />
        public async ValueTask<bool> WaitAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            _attempts++;

            var elapsed = (int) (DateTime.UtcNow - _begin).TotalMilliseconds;

            if (elapsed > _timeout)
            {
                _logger.LogWarning($"Unable to {_action} after {_attempts} attempts and {_timeout} ms, giving up.");
                return false;
            }

            var delay = (int) (_currentBackOff * (1 - _jitter * (1 - RandomProvider.Random.NextDouble())));
            delay = Math.Min(delay, (int) (_timeout - elapsed));

            _logger.LogDebug($"Unable to {_action} after {_attempts} attempts and {elapsed} ms, retrying in {delay} ms");

            try
            {
                await Task.Delay(delay, cancellationToken).CAF();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Unable to {_action} after {_attempts} attempts and {_timeout} ms, was cancelled.");
                return false;
            }
            catch (Exception)
            {
                _logger.LogWarning($"Unable to {_action} after {_attempts} attempts and {_timeout} ms, error.");
                return false;
            }

            _currentBackOff = (int) Math.Min(_currentBackOff * _multiplier, _maxBackOff);
            return true;
        }

        /// <inheritdoc />
        public void Restart()
        {
            _attempts = 0;
            _currentBackOff = Math.Min(_maxBackOff, _initialBackOff);
            _begin = DateTime.UtcNow;
        }
    }
}
