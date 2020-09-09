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
using Hazelcast.Exceptions;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    [TestFixture]
    public class BufferPoolTest
    {
        private ISerializationService _serializationService;
        private BufferPoolThreadLocal _bufferPoolThreadLocal;

        [SetUp]
        public void Setup()
        {
            _serializationService = new SerializationServiceBuilder(new NullLoggerFactory()).Build();
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

            _bufferPoolThreadLocal.Dispose();

            Assert.Throws<ClientNotConnectedException>(() => _bufferPoolThreadLocal.Get());
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
            for (var i = 0; i < 100_000; i++)
            {
                CreateGetAndDispose();
                AssertMemoryLimit(i);
            }
        }

        [Test]
        public void ThreadLocal_Finalizer()
        {
            for (var i = 0; i < 100_000; i++)
            {
                CreateGetAndLeaveForFinalizer();

                if (i % 1000 == 0)
                {
                    GC.WaitForPendingFinalizers();
                }

                AssertMemoryLimit(i);
            }
        }

        const long MemoryLimit = 256 * 1024 * 1024;

        static readonly byte[] ALotOfBytes = new byte[64 * 1024];

        void CreateGetAndDispose()
        {
            using (var local = new BufferPoolThreadLocal(_serializationService))
            {
                WriteALotOfBytes(local);
            }
        }

        static void AssertMemoryLimit(int iteration)
        {
            // assert every 1000 iterations
            if (iteration % 1000 == 0)
            {
                Assert.Greater(MemoryLimit, GC.GetTotalMemory(true), "Memory limit breached. It looks like there's a memory leak in pool management.");
            }
        }

        void CreateGetAndLeaveForFinalizer()
        {
            WriteALotOfBytes(new BufferPoolThreadLocal(_serializationService));
        }

        static void WriteALotOfBytes(BufferPoolThreadLocal local)
        {
            var pool = local.Get();
            var buffer = pool.TakeOutputBuffer();
            buffer.WriteBytes(ALotOfBytes);
            pool.ReturnOutputBuffer(buffer);
        }
    }
}