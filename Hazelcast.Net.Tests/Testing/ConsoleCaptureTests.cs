using System;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    public class ConsoleCaptureTests
    {
        [Test]
        public void Captures()
        {
            var capture = new ConsoleCapture();
            using (capture.Output())
            {
                Console.WriteLine("test");
            }
            Assert.AreEqual("test\r\n", capture.ReadToEnd());
        }
    }
}
