// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
    public class ClientAtomicLongTest : SingleMemberBaseTest
    {
        //
        [SetUp]
        public void Init()
        {
            l = Client.GetAtomicLong(TestSupport.RandomString());
            l.Set(0);
        }

        [TearDown]
        public static void Destroy()
        {
            l.Destroy();
        }

        internal const string name = "ClientAtomicLongTest";

        internal static IAtomicLong l;

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void Test()
        {
            Assert.AreEqual(0, l.GetAndAdd(2));
            Assert.AreEqual(2, l.Get());
            l.Set(5);
            Assert.AreEqual(5, l.Get());
            Assert.AreEqual(8, l.AddAndGet(3));
            Assert.IsFalse(l.CompareAndSet(7, 4));
            Assert.AreEqual(8, l.Get());
            Assert.IsTrue(l.CompareAndSet(8, 4));
            Assert.AreEqual(4, l.Get());
            Assert.AreEqual(3, l.DecrementAndGet());
            Assert.AreEqual(3, l.GetAndIncrement());
            Assert.AreEqual(4, l.GetAndSet(9));
            Assert.AreEqual(10, l.IncrementAndGet());
        }
    }
}