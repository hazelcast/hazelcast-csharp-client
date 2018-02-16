﻿// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
            _bufferPoolThreadLocal.Get();

            // we kill all strong references.
            _bufferPoolThreadLocal.Dispose();

            TestSupport.AssertTrueEventually(() =>
            {
                GC.Collect();
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
            var poolRef = new WeakReference(_bufferPoolThreadLocal.Get());

            // call clear; kills the strong references.
            _bufferPoolThreadLocal.Dispose();

            TestSupport.AssertTrueEventually(() =>
            {
                GC.Collect();
                Assert.Null(poolRef.Target);
            });
        }
    }
}