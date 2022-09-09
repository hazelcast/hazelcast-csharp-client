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
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class ServerVersionTests
    {
        // in ServerVersion order is:
        // 1. use the detector, which will return non-null if it *can* detect the version
        // 2. use the supplied default version
        // 3. fallback to the hard-coded default version (but, really?)

        [Test]
        public void DetectVersion()
        {
            var version = ServerVersionDetector.DetectedServerVersion;
            var versionString = "NULL";
            if (version != null) versionString = version.ToString();

            // this should be easy to grep in the test output
            Console.WriteLine($"[[[DetectedServerVersion:{versionString}]]]");
        }

        [Test]
        public void Detector()
        {
            using var forced = ServerVersionDetector.ForceVersion("7.7-TESTING");
            Assert.AreEqual(NuGetVersion.Parse("7.7-TESTING"), ServerVersion.GetVersion());
        }

        [Test]
        public void SuppliedDefault()
        {
            using var forced = ServerVersionDetector.ForceNoVersion();
            Assert.AreEqual(NuGetVersion.Parse("8.8-TESTING"), ServerVersion.GetVersion("8.8-TESTING"));
        }

        [Test]
        public void HardCodedDefault()
        {
            using var forced = ServerVersionDetector.ForceNoVersion();
            Assert.AreEqual(ServerVersion.DefaultVersion, ServerVersion.GetVersion());
        }
    }
}
