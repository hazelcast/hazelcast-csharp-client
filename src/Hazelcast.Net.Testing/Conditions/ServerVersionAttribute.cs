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
using System;
using NuGet.Versioning;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Specifies that a class or a method overrides the server version.
    /// </summary>
    /// <remarks>
    /// <para>This attributes takes precedence over all other methods of defining the
    /// server version.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public sealed class ServerVersionAttribute :  Attribute, ITestAction
    {
        public const string PropertyName = "ServerVersionAttribute.Version";

        /// <summary>
        /// Specifies that a class or a method overrides the server version.
        /// </summary>
        /// <param name="version">The server version.</param>
        public ServerVersionAttribute(string version)
        {
            if (!NuGetVersion.TryParse(version, out var v))
                throw new ArgumentException("Invalid version.", nameof(version));

            Version = v;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public NuGetVersion Version { get; }

        // this attribute used to implement IApplyToTest but then, NUnit creates one test context for
        // OneTimeSetUp phase and another one for the actual test method, and does not copy properties
        // over from one to the other - and don't mention test suites (created by [TestCase(...)]) that
        // don't even seem to have their own setup method, so [ServerVersion] on them would fail.

        /*
        /// <inheritdoc />
        public void ApplyToTest(Test test)
        {
            test.Properties[PropertyName] = new[] { Version };
        }
        */

        // instead, we make this attribute implement ITestAction and trigger on tests and on suites,
        // which seem to include fixtures too, *and* seem to run on the same bag of properties, so in
        // the end - it works.

        /// <inheritdoc />
        public void BeforeTest(ITest test)
        {
            test.Properties[PropertyName] = new[] { Version };
        }

        /// <inheritdoc />
        public void AfterTest(ITest test)
        { }

        /// <inheritdoc />
        public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;
    }
}
