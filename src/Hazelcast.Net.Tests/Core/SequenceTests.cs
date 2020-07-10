using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class SequenceTests
    {
        [Test]
        public void Int32Sequence()
        {
            ISequence<int> sequence = new Int32Sequence();

            for (var i = 0; i < 100; i++)
                Assert.That(sequence.GetNext(), Is.EqualTo(i+1));
        }

        [Test]
        public void Int64Sequence()
        {
            ISequence<long> sequence = new Int64Sequence();

            for (var i = 0; i < 100; i++)
                Assert.That(sequence.GetNext(), Is.EqualTo(i+1));
        }
    }
}
