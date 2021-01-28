// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// Implements a <see cref="IRetryStrategy"/> with back-off and timeout.
    /// </summary>
    internal class RetryStrategy : IRetryStrategy
    {
        private readonly int _initialBackOffMilliseconds;
        private readonly int _maxBackOffMilliseconds;
        private readonly double _multiplier;
        private readonly long _timeoutMilliseconds;
        private readonly double _jitter;
        private readonly string _action;
        private readonly ILogger _logger;

        private int _currentBackOffMilliseconds;
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
                options.TimeoutMilliseconds,
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
            _action = action.ToLowerInvariant();
            _initialBackOffMilliseconds = initialBackOffMilliseconds;
            _maxBackOffMilliseconds = maxBackOffMilliseconds;
            _multiplier = multiplier;
            _timeoutMilliseconds = timeoutMilliseconds;
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

            if (elapsed > _timeoutMilliseconds)
            {
                _logger.LogWarning($"Unable to {_action} after {_attempts} attempts and {_timeoutMilliseconds} ms, giving up.");
                return false;
            }

            var delay = (int) (_currentBackOffMilliseconds * (1 - _jitter * (1 - RandomProvider.Random.NextDouble())));
            delay = Math.Min(delay, (int) (_timeoutMilliseconds - elapsed));

            _logger.LogDebug($"Unable to {_action} after {_attempts} attempts and {elapsed} ms, retrying in {delay} ms");

            try
            {
                await Task.Delay(delay, cancellationToken).CfAwait();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Unable to {_action} after {_attempts} attempts and {_timeoutMilliseconds} ms, was cancelled.");
                return false;
            }
            catch (Exception)
            {
                _logger.LogWarning($"Unable to {_action} after {_attempts} attempts and {_timeoutMilliseconds} ms, error.");
                return false;
            }

            _currentBackOffMilliseconds = (int) Math.Min(_currentBackOffMilliseconds * _multiplier, _maxBackOffMilliseconds);
            return true;
        }

        /// <inheritdoc />
        public void Restart()
        {
            _attempts = 0;
            _currentBackOffMilliseconds = Math.Min(_maxBackOffMilliseconds, _initialBackOffMilliseconds);
            _begin = DateTime.UtcNow;
        }
    }
}
