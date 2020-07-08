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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;
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

        private readonly ConcurrentQueue<UnobservedTaskExceptionEventArgs> _unobservedExceptions =
            new ConcurrentQueue<UnobservedTaskExceptionEventArgs>();

        [SetUp]
        public void HazelcastTestBaseSetUp()
        {
            // creating the client via an async method means we may not have a context - ensure here
            AsyncContext.Ensure();

            // setup the logger
            LoggerFactory = CreateLoggerFactory();
            Logger = LoggerFactory.CreateLogger(GetType());
            Logger.LogInformation($"Setup {GetType()}");

            // make sure the queue is empty
            while (_unobservedExceptions.TryDequeue(out _))
            { }

            // handle unobserved exceptions
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        [TearDown]
        public void HazelcastTestBaseTearDown()
        {
            // GC should finalize everything, thus trigger unobserved exceptions
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // check for unobserved exceptions and report
            var failed = false;
            while (_unobservedExceptions.TryDequeue(out var args))
            {
                var innerException = args.Exception.Flatten().InnerException;
                Logger.LogWarning(innerException, "Exception.");
                failed = true;
            }

            // remove handler
            TaskScheduler.UnobservedTaskException -= UnobservedTaskException;

            // fail if necessary
            if (failed) Assert.Fail("Unobserved task exceptions.");
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.LogWarning(e.Exception, $"UnobservedTaskException from {sender}.");
            _unobservedExceptions.Enqueue(e);
        }

        /// <summary>
        /// Gets the default test timeout in milliseconds.
        /// </summary>
        public const int TestTimeoutMilliseconds = 20_000;

        /// <summary>
        /// Provides assertions.
        /// </summary>
        protected Asserter Assert { get; } = new Asserter();

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; private set; }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Creates a unique name.
        /// </summary>
        /// <returns>A unique name.</returns>
        /// <remarks>
        /// <para>The unique name is unique across all tests within a run, and to
        /// a certain extend also unique across all runs.</para>
        /// </remarks>
        protected string CreateUniqueName() => UniqueNamePrefix + UniqueNameSequence.GetNext();

        /// <summary>
        /// Creates a logger factory.
        /// </summary>
        /// <returns>A logger factory.</returns>
        protected virtual ILoggerFactory CreateLoggerFactory() => 
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
    }
}
