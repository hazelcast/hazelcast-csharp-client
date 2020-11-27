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
using System.Linq;
using System.Reflection;
using Hazelcast.Testing.Conditions;
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class ServerVersionTests
    {
        [Test]
        public void VersionSources()
        {
            Environment.SetEnvironmentVariable(ServerVersion.EnvironmentVariableName, "");
            Assert.AreEqual(ServerVersion.DefaultVersion, ServerVersion.GetVersion());
            Assert.AreEqual(TestAssemblyServerVersion, ServerVersion.GetVersion(TestAssemblyServerVersion));

            Environment.SetEnvironmentVariable(ServerVersion.EnvironmentVariableName, "0.6");
            Assert.AreEqual(NuGetVersion.Parse("0.6"), ServerVersion.GetVersion());
            Assert.AreEqual(NuGetVersion.Parse("0.6"), ServerVersion.GetVersion(TestAssemblyServerVersion));

            Environment.SetEnvironmentVariable(ServerVersion.EnvironmentVariableName, "");
            Assert.AreEqual(ServerVersion.DefaultVersion, ServerVersion.GetVersion());
            Assert.AreEqual(TestAssemblyServerVersion, ServerVersion.GetVersion(TestAssemblyServerVersion));
        }

        // gets the version indicated by the [assembly:ServerVersion()] in AssemblyInfo.cs
        private NuGetVersion TestAssemblyServerVersion
            => GetType().Assembly.GetCustomAttributes<ServerVersionAttribute>().FirstOrDefault()?.Version;
    }
}
