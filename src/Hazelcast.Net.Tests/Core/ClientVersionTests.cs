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
using System.Reflection;
using Hazelcast.Clustering;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ClientVersionTests
    {
        [Test]
        public void WriteVersions()
        {
            Console.WriteLine(ClientVersion.Version);
            Console.WriteLine(ClientVersion.GetSemVerWithoutBuildingMetadata());
        }


        [Test]
        public void ClientVersionReturnsVersion()
        {
            // both attributes missing, returns 0.0.0
            Assert.That(ClientVersion.GetVersion(null, null), Is.EqualTo("0.0.0"));

            // informational version missing, uses version
            Assert.That(ClientVersion.GetVersion(null, new AssemblyVersionAttribute("1.2.3")), Is.EqualTo("1.2.3"));

            // informational version present, uses it
            Assert.That(ClientVersion.GetVersion(new AssemblyInformationalVersionAttribute("1.2.3"), null), Is.EqualTo("1.2.3"));
            Assert.That(ClientVersion.GetVersion(new AssemblyInformationalVersionAttribute("1.2.3-preview.0"), null), Is.EqualTo("1.2.3-preview.0"));
            Assert.That(ClientVersion.GetVersion(new AssemblyInformationalVersionAttribute("1.2.3+ae12b5"), null), Is.EqualTo("1.2.3+ae12b5"));
            Assert.That(ClientVersion.GetVersion(new AssemblyInformationalVersionAttribute("1.2.3-preview.0+ae12b5"), null), Is.EqualTo("1.2.3-preview.0+ae12b5"));

            Assert.That(ClientVersion.GetVersion(new AssemblyInformationalVersionAttribute("1.2.3+ae12b5d9dc12f9efd3"), null), Is.EqualTo("1.2.3+ae12b5"));
            Assert.That(ClientVersion.GetVersion(new AssemblyInformationalVersionAttribute("1.2.3-preview.0+ae12b5d9dc12f9efd3"), null), Is.EqualTo("1.2.3-preview.0+ae12b5"));

            // both present, ignores version
            Assert.That(ClientVersion.GetVersion(new AssemblyInformationalVersionAttribute("1.2.3"), new AssemblyVersionAttribute("4.5.6")), Is.EqualTo("1.2.3"));


            Assert.That(Authenticator.ClientVersion, Is.EqualTo(ClientVersion.MajorMinorPatchVersion));
            Assert.That(Authenticator.ClientVersion.Split('.').Length, Is.GreaterThanOrEqualTo(3));
        }


        [TestCase("1.2.3", "1.2.3")]
        [TestCase("1.2.0", "1.2.0")]
        [TestCase("1.2.3-preview.0", "1.2.3")]
        [TestCase("1.2.0-preview.0", "1.2.0")]
        [TestCase("1.2.3-SNAPSHOT", "1.2.3")]
        [TestCase("1.2.3+ae12b5d9", "1.2.3")]
        [TestCase("1.2.3-preview.0+ae12b5", "1.2.3")]
        public void ClientVersionReturnsMajorMinorPatchVersion(string semverVersion, string expectedVersion)
        {
            Assert.That(ClientVersion.GetMajorMinorPatchVersion(semverVersion), Is.EqualTo(expectedVersion));
        }

        [TestCase("1.2.3", "1.2.3")]
        [TestCase("1.2.3-SNAPSHOT", "1.2.3-SNAPSHOT")]
        [TestCase("1.2.3-preview.0", "1.2.3-preview.0")]
        [TestCase("1.2.3+ae12b5d9", "1.2.3")]
        [TestCase("1.2.3-preview.0+ae12b5", "1.2.3-preview.0")]
        public void TestGetSemVerWithoutBuildingMetadata(string semverVersion, string expectedVersion)
        {
            Assert.That(ClientVersion.GetSemVerWithoutBuildingMetadata(semverVersion), Is.EqualTo(expectedVersion));
        }
        
    }
}
