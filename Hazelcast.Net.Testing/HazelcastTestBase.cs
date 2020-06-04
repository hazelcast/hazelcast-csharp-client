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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests.
    /// </summary>
    public abstract class HazelcastTestBase
    {
        private static readonly ISequence<int> UniqueNameSequence = new Int32Sequence();
        private static readonly string UniqueNamePrefix = DateTime.Now.ToString("HHmmss_");

        private readonly List<object> _disposables = new List<object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastTestBase"/> class.
        /// </summary>
        protected HazelcastTestBase()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            LoggerFactory = CreateLoggerFactory();

            Logger = LoggerFactory.CreateLogger(GetType());
        }

        [TearDown]
        public async Task HaselcastTestBaseTearDown()
        {
            List<Exception> exceptions = null;
            foreach (var disposable in _disposables)
            {
                try
                {
                    switch (disposable)
                    {
                        case IDisposable syncDisposable:
                            syncDisposable.Dispose();
                            break;
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync().CAF();
                            break;
                        default:
                            throw new NotSupportedException("Object is neither IDisposable nor IAsyncDisposable.");
                    }
                }
                catch (Exception e)
                {
                    if (exceptions == null) exceptions = new List<Exception>();
                    exceptions.Add(e);
                }
            }

            _disposables.Clear();

            if (exceptions != null)
                throw new AggregateException("Exceptions while disposing disposables.", exceptions.ToArray());
        }

        /// <summary>
        /// Gets the default test timeout in milliseconds.
        /// </summary>
        public const int TimeoutMilliseconds = 20_000;

        /// <summary>
        /// Provides assertions.
        /// </summary>
        protected Asserter Assert { get; } = new Asserter();

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Creates a unique name.
        /// </summary>
        /// <returns>A unique name.</returns>
        /// <remarks>
        /// <para>The unique name is unique across all tests within a run, and to
        /// a certain extend also unique across all runs.</para>
        /// </remarks>
        protected string CreateUniqueName() => UniqueNamePrefix + UniqueNameSequence.Next;

        /// <summary>
        /// Creates a logger factory.
        /// </summary>
        /// <returns>A logger factory.</returns>
        protected virtual ILoggerFactory CreateLoggerFactory() => new NullLoggerFactory();

        /// <summary>
        /// Registers an object to be disposed at the end of the test.
        /// </summary>
        /// <param name="disposable">A disposable object.</param>
        protected void AddDisposable(IDisposable disposable)
            => _disposables.Add(disposable);

        /// <summary>
        /// Registers an object to be disposed at the end of the test.
        /// </summary>
        /// <param name="disposable">A disposable object.</param>
        protected void AddDisposable(IAsyncDisposable disposable)
            => _disposables.Add(disposable);

        /// <summary>
        /// De-registers an object which was to be disposed at the end of the test.
        /// </summary>
        /// <param name="disposable">The disposable object.</param>
        protected void RemoveDisposable(IDisposable disposable)
            => _disposables.Remove(disposable);

        /// <summary>
        /// De-registers an object which was to be disposed at the end of the test.
        /// </summary>
        /// <param name="disposable">The disposable object.</param>
        protected void RemoveDisposable(IAsyncDisposable disposable)
            => _disposables.Remove(disposable);
    }
}
