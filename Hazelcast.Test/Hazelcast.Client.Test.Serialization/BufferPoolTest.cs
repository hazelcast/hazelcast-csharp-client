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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    [TestFixture]
    public class BufferPoolTest
    {
        private ISerializationService _serializationService;
        private BufferPoolThreadLocal _bufferPoolThreadLocal;

        [SetUp]
        public void Setup()
        {
            _serializationService = new SerializationServiceBuilder().Build();
            _bufferPoolThreadLocal = new BufferPoolThreadLocal(_serializationService);
        }

        [TearDown]
        public void Teardown()
        {
            _serializationService.Destroy();
            _bufferPoolThreadLocal.Dispose();
        }

        [Test]
        public void Get_whenSameThread_samePoolInstance()
        {
            var pool1 = _bufferPoolThreadLocal.Get();
            var pool2 = _bufferPoolThreadLocal.Get();
            Assert.AreSame(pool1, pool2);
        }

        [Test]
        public void Get_whenDifferentThreads_thenDifferentInstances()
        {
            var pool1 = _bufferPoolThreadLocal.Get();
            var pool2 = Task<BufferPool>.Factory.StartNew(() => _bufferPoolThreadLocal.Get()).Result;
            Assert.AreNotSame(pool1, pool2);
        }

        [Test]
        public void Get_whenCleared()
        {
            // forces the creation of a bufferpool.
            // don't get it directly, else the GC/JIT may decide to keep it around
            //_bufferPoolThreadLocal.Get();
            GetFromBufferPoolThreadLocal(_bufferPoolThreadLocal);

            // we kill all strong references.
            _bufferPoolThreadLocal.Dispose();

            TestSupport.AssertTrueEventually(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                try
                {
                    _bufferPoolThreadLocal.Get();
                    Assert.Fail();
                }
                catch (HazelcastInstanceNotActiveException)
                {
                }
            });
        }

        private static void GetFromBufferPoolThreadLocal(BufferPoolThreadLocal bptl)
        {
            bptl.Get();
        }

        [Test]
        public void Get_whenDifferentThreadLocals_thenDifferentInstances()
        {
            var bufferPoolThreadLocal2 = new BufferPoolThreadLocal(_serializationService);

            var pool1 = _bufferPoolThreadLocal.Get();
            var pool2 = bufferPoolThreadLocal2.Get();
            Assert.AreNotSame(pool1, pool2);
        }

        [Test]
        public void ThreadLocal_Dispose()
        {
            // store the pool in a weak reference since we don't want to force a strong reference ourselves.
            // don't get it directly, else the GC/JIT may decide to keep it around
            //var poolRef = new WeakReference(_bufferPoolThreadLocal.Get());
            var poolRef = GetWeakReference(_bufferPoolThreadLocal);

            // call clear; kills the strong references.
            _bufferPoolThreadLocal.Dispose();

            TestSupport.AssertTrueEventually(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Assert.IsFalse(poolRef.IsAlive);
            });
        }

        private static WeakReference GetWeakReference(BufferPoolThreadLocal bptl)
        {
            return new WeakReference(bptl.Get());
        }
    }
}