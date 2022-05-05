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
        private RetryOptions _retryOptions;

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

            _retryOptions = new RetryOptions(initialBackOffMilliseconds, maxBackOffMilliseconds, multiplier, timeoutMilliseconds, jitter);
            _currentBackOffMilliseconds = initialBackOffMilliseconds;

            _logger = loggerFactory?.CreateLogger<RetryStrategy>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            Restart();
        }

        /// <inheritdoc />
        public async ValueTask<bool> WaitAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            _attempts++;

            var elapsed = (int)(DateTime.UtcNow - _begin).TotalMilliseconds;

            _logger.IfDebug()?.LogDebug("Elapsed {Elapsed}ms, timeout={TimeoutMilliseconds}ms, back-off={CurrentBackOffMilliseconds}ms ({InitialBackOffMilliseconds}-{MaxBackOffMilliseconds}, m={Multiplier}, j={Jitter})", elapsed, _retryOptions.TimeoutMilliseconds, _currentBackOffMilliseconds, _retryOptions.InitialBackoffMilliseconds, _retryOptions.MaxBackoffMilliseconds, _retryOptions.Multiplier, _retryOptions.Jitter);

            if (_retryOptions.TimeoutMilliseconds > 0 && elapsed > _retryOptions.TimeoutMilliseconds)
            {
                _logger.IfWarning()?.LogWarning("Unable to {Action} after {Attempts} attempts and {Elapsed}ms, timeout ({TimeoutMilliseconds}ms).", _action, _attempts, elapsed, _retryOptions.TimeoutMilliseconds);
                return false;
            }

            var delay = (int)(_currentBackOffMilliseconds * (1 - _retryOptions.Jitter * (1 - RandomProvider.NextDouble())));
            delay = Math.Min(delay, Math.Max(0, (int)(_retryOptions.TimeoutMilliseconds - elapsed)));

            _logger.IfDebug()?.LogDebug("Unable to {Action} after {Attempts} attempts and {Elapsed}ms, will retry in {Delay}ms", _action, _attempts, elapsed, delay);

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

            _currentBackOffMilliseconds = (int)Math.Min(_currentBackOffMilliseconds * _retryOptions.Multiplier, _retryOptions.MaxBackoffMilliseconds);
            return true;
        }

        /// <inheritdoc />
        public void Restart()
        {
            _attempts = 0;
            _currentBackOffMilliseconds = Math.Min(_retryOptions.MaxBackoffMilliseconds, _retryOptions.InitialBackoffMilliseconds);
            _begin = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public void ChangeStrategy(ConnectionRetryOptions options)
        {
            var newOptions = new RetryOptions(options.InitialBackoffMilliseconds,
                options.MaxBackoffMilliseconds,
                options.Multiplier,
                options.ClusterConnectionTimeoutMilliseconds,
                options.Jitter);

            _retryOptions = newOptions;
        }

        /// <summary>
        /// Internal structure of Retry strategy. It is used to work on options atomically.
        /// </summary>
        private struct RetryOptions
        {
            public RetryOptions(int initialBackoffMilliseconds, int maxBackoffMilliseconds, double multiplier, long timeoutMilliseconds, double jitter)
            {
                InitialBackoffMilliseconds = initialBackoffMilliseconds;
                MaxBackoffMilliseconds = maxBackoffMilliseconds;
                Multiplier = multiplier;
                TimeoutMilliseconds = timeoutMilliseconds;
                Jitter = jitter;
            }

            public int InitialBackoffMilliseconds { get; }
            public int MaxBackoffMilliseconds { get; }
            public double Multiplier { get; }
            public long TimeoutMilliseconds { get; }
            public double Jitter { get; }
        }
    }
}
