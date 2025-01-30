// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests.
    /// </summary>
    public abstract class HazelcastTestBase : ObservingTestBase
    {
        private static readonly ISequence<int> UniqueNameSequence = new Int32Sequence();
        private static readonly string UniqueNamePrefix = "x_" + DateTime.Now.ToString("HHmmss_");

        [OneTimeSetUp]
        public void HazelcastTestBaseOneTimeRootSetUp()
        {
            // setup the logger
            LoggerFactory = CreateLoggerFactory();
            Logger = LoggerFactory.CreateLogger(GetType());
            Logger.LogInformation($"Setup {GetType()}");

            // start fresh
            HConsole.Configure(x => x.ClearAll());

            // top-level overrides
            HazelcastTestBaseOneTimeSetUp();
        }

        //[OneTimeSetUp]
        public virtual void HazelcastTestBaseOneTimeSetUp()
        { }

        //[OneTimeTearDown]
        public void HazelcastTestBaseOneTimeRootTearDown()
        { }

        [SetUp]
        public void HazelcastTestBaseSetUp()
        {
            // creating the client via an async method means we may not have a context - ensure here
            // (before each test)
            AsyncContext.RequireNew();

            // reset the HConsole options
            HConsole.Reset();
        }

        [TearDown]
        public void HazelcastTestBaseTearDown()
        {
            // in case it's been used by tests, reset the clock
            // (after each test)
            Clock.Reset();
        }

        protected override void ReportUnobservedException(string message, Exception exception)
        {
            Logger.LogWarning(exception, message);
        }

        /// <summary>
        /// Gets the default test timeout in milliseconds.
        /// </summary>
        public const int TestTimeoutMilliseconds = 20_000;

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
        
        /// <summary>
        /// Gets a random name.
        /// </summary>
        protected static string GetRandomName(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..7]}";
    }
}