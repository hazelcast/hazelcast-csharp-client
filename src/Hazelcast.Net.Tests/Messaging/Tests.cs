using Hazelcast.Messaging;
using NUnit.Framework;

namespace Hazelcast.Tests.Messaging
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Options()
        {
            var options = new MessagingOptions
            {
                DefaultOperationTimeoutMilliseconds = 123,
                MinRetryDelayMilliseconds = 456,
                MaxFastInvocationCount = 789
            };

            Assert.That(options.DefaultOperationTimeoutMilliseconds, Is.EqualTo(123));
            Assert.That(options.MinRetryDelayMilliseconds, Is.EqualTo(456));
            Assert.That(options.MaxFastInvocationCount, Is.EqualTo(789));

            var clone = options.Clone();

            Assert.That(clone.DefaultOperationTimeoutMilliseconds, Is.EqualTo(123));
            Assert.That(clone.MinRetryDelayMilliseconds, Is.EqualTo(456));
            Assert.That(clone.MaxFastInvocationCount, Is.EqualTo(789));
        }

        [Test]
        public void FlagsExtensions()
        {
            Assert.That(FrameFlags.Default.ToBetterString(), Is.EqualTo("Default"));

            Assert.That(FrameFlags.BeginStruct.ToBetterString(), Is.EqualTo("BeginStruct"));
            Assert.That((FrameFlags.BeginStruct | FrameFlags.Final).ToBetterString(), Is.EqualTo("BeginStruct, Final"));

            Assert.That(ClientMessageFlags.BackupAware.ToBetterString(), Is.EqualTo("BackupAware"));
            Assert.That((ClientMessageFlags.BackupAware | ClientMessageFlags.Event).ToBetterString(), Is.EqualTo("BackupAware, Event"));

            Assert.That((FrameFlags.Final | (FrameFlags) ClientMessageFlags.Event).ToBetterString(), Is.EqualTo("Final, Event"));
            Assert.That(((ClientMessageFlags) FrameFlags.Final | ClientMessageFlags.Event).ToBetterString(), Is.EqualTo("Final, Event"));
        }
    }
}
