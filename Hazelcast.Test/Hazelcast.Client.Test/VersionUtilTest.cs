// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class VersionUtilTest
    {
        [Test]
        public void DllVersion()
        {
            var assemblyVersion = typeof(VersionUtil).Assembly.GetName().Version.ToString();
            var extracted = VersionUtil.GetDllVersion();

            Assert.That(assemblyVersion, Does.StartWith(extracted));
        }

        [Test]
        public void ServerVersionParse()
        {
            Assert.AreEqual(VersionUtil.UnknownVersion, VersionUtil.ParseServerVersion("3"));
            Assert.AreEqual(30900, VersionUtil.ParseServerVersion("3.9"));
            Assert.AreEqual(30901, VersionUtil.ParseServerVersion("3.9.1"));
            Assert.AreEqual(30900, VersionUtil.ParseServerVersion("3.9-SNAPSHOT"));
            Assert.AreEqual(30901, VersionUtil.ParseServerVersion("3.9.1-SNAPSHOT"));
        }
    }
}