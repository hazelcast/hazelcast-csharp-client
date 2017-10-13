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