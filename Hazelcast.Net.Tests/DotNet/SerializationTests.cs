using System;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void AssertGuidByteOrder()
        {
            var guid = new Guid("00010203-0405-0607-0809-0a0b0c0d0e0f");
            var a = guid.ToByteArray();

            // verify the order of Guid bytes in the byte array
            Assert.AreEqual(0x03, a[00]);
            Assert.AreEqual(0x02, a[01]);
            Assert.AreEqual(0x01, a[02]);
            Assert.AreEqual(0x00, a[03]);
            Assert.AreEqual(0x05, a[04]);
            Assert.AreEqual(0x04, a[05]);
            Assert.AreEqual(0x07, a[06]);
            Assert.AreEqual(0x06, a[07]);
            Assert.AreEqual(0x08, a[08]);
            Assert.AreEqual(0x09, a[09]);
            Assert.AreEqual(0x0a, a[10]);
            Assert.AreEqual(0x0b, a[11]);
            Assert.AreEqual(0x0c, a[12]);
            Assert.AreEqual(0x0d, a[13]);
            Assert.AreEqual(0x0e, a[14]);
            Assert.AreEqual(0x0f, a[15]);
        }
    }
}
