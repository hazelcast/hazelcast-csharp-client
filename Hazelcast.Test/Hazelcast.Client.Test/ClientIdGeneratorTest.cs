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

using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientIdGeneratorTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            i = Client.GetIdGenerator(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            i.Destroy();
        }

        internal const string name = "ClientIdGeneratorTest";

        internal static IIdGenerator i;

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestGenerator()
        {
            Assert.IsFalse(i.Init(-4569));

            Assert.IsTrue(i.Init(3569));
            Assert.IsFalse(i.Init(4569));
            Assert.AreEqual(3570, i.NewId());
        }

        [Test]
        public virtual void TestGeneratorBlockSize()
        {
            for (var j = 0; j < 10000; j++)
            {
                i.NewId();
            }

            Assert.AreEqual(10000, i.NewId());
            Assert.AreEqual(10001, i.NewId());
        }
    }
}