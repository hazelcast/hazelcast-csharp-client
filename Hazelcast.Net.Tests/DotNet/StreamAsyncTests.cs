using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class StreamAsyncTests
    {
        [Test]
        [Timeout(10_000)]
        public async Task CanCancelRead()
        {
            Stream stream = new MemoryStream();

            var memory = new Memory<byte>(new byte[256]);

            var source = new CancellationTokenSource();
            //source.CancelAfter(2000);

            // wtf is this non-blocking?!
            var count = await stream.ReadAsync(memory, source.Token);
            Console.WriteLine(count);

            // should end ok
        }
    }
}
