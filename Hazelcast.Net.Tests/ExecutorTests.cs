using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Implementation.Executor;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class ExecutorTests
    {
        public class HelloExecutable : IExecutable<string>
        {
            public string Execute()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        [Ignore("Not implemented.")]
        public async Task Test()
        {
            var executor = new Executor(null, null, null, null, null);
            var result = await executor.ExecuteAsync(new HelloExecutable(), CancellationToken.None);
            Assert.AreEqual("hello", result);
        }
    }
}
