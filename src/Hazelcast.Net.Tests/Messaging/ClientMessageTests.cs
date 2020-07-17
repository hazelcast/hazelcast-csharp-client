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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Messaging;
using NUnit.Framework;

namespace Hazelcast.Tests.Messaging
{
    [TestFixture]
    public class ClientMessageTests
    {
        [Test]
        public void Properties()
        {
            var m = new ClientMessage();
            Assert.That(m.FirstFrame, Is.Null);
            Assert.That(m.LastFrame, Is.Null);

            Assert.Throws<ArgumentNullException>(() => m.Append(null));

            var f = new Frame(new byte[256]);
            m = new ClientMessage(f);
            Assert.That(m.FirstFrame, Is.SameAs(f));
            Assert.That(m.LastFrame, Is.SameAs(f));
            Assert.That(f.Next, Is.Null);

            m.Flags = ClientMessageFlags.Default;
            Assert.That(m.Flags, Is.EqualTo(ClientMessageFlags.Default));

            m.PartitionId = 123;
            Assert.That(m.PartitionId, Is.EqualTo(123));

            m.CorrelationId = 456;
            Assert.That(m.CorrelationId, Is.EqualTo(456));

            m.MessageType = 789;
            Assert.That(m.MessageType, Is.EqualTo(789));

            m.OperationName = "op";
            Assert.That(m.OperationName, Is.EqualTo("op"));

            m.FragmentId = 552;
            Assert.That(m.FragmentId, Is.EqualTo(552));

            m.Flags = ClientMessageFlags.Event;
            Assert.That(m.IsEvent);

            m.Flags = ClientMessageFlags.BackupAware;
            Assert.That(m.IsBackupAware);

            m.Flags = ClientMessageFlags.BackupEvent;
            Assert.That(m.IsBackupEvent);

            m.MessageType = 0;
            Assert.That(m.IsException);
            m.MessageType = 1;
            Assert.That(m.IsException, Is.False);

            Assert.That(m.IsRetryable, Is.False);
            m.IsRetryable = true;
            Assert.That(m.IsRetryable);
        }

        [Test]
        public void CloneWithNewCorrelationId()
        {
            Assert.Throws<ArgumentNullException>(() => _ = ((ClientMessage) null).CloneWithNewCorrelationId(1));

            var m = new ClientMessage();
            var bytes1 = new byte[256];
            m.Append(new Frame(bytes1));
            var bytes2 = new byte[256];
            m.Append(new Frame(bytes2));
            m.Append(new Frame());

            m.Flags = ClientMessageFlags.BackupAware;
            m.PartitionId = 141;
            m.OperationName = "op";
            m.MessageType = 789;
            m.CorrelationId = 123;

            var clone = m.CloneWithNewCorrelationId(456);

            Assert.That(clone.Flags, Is.EqualTo(m.Flags));
            Assert.That(clone.PartitionId, Is.EqualTo(m.PartitionId));
            Assert.That(clone.OperationName, Is.EqualTo(m.OperationName));
            Assert.That(clone.MessageType, Is.EqualTo(m.MessageType));
            Assert.That(clone.CorrelationId, Is.EqualTo(456));

            Assert.That(clone.FirstFrame.Bytes, Is.Not.SameAs(bytes1));
            Assert.That(clone.FirstFrame.Next.Bytes, Is.SameAs(bytes2));
        }

        [Test]
        public void Enumerator()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerator<Frame>) null).SkipToStructEnd());
            Assert.Throws<ArgumentNullException>(() => _ = ((IEnumerator<Frame>) null).SkipNull());
            Assert.Throws<ArgumentNullException>(() => _ = ((IEnumerator<Frame>) null).Take());

            var frames = new Frame[8];
            for (var i = 0; i < 8; i++)
                frames[i] = new Frame(new byte[64]);

            var m = new ClientMessage();
            for (var i = 0; i < 8; i++)
                m.Append(frames[i]);

            var enumerated = m.ToList();
            for (var i = 0; i < 8; i++)
                Assert.That(enumerated[i], Is.SameAs(frames[i]));

            IEnumerable em = m;
            enumerated.Clear();
            foreach (Frame x in em)
                enumerated.Add(x);
            for (var i = 0; i < 8; i++)
                Assert.That(enumerated[i], Is.SameAs(frames[i]));

            frames[2].Flags = FrameFlags.BeginStruct;
            frames[5].Flags = FrameFlags.EndStruct;

            using (var e = m.GetEnumerator())
            {
                e.MoveNext();
                while (!e.Current.IsBeginStruct) e.MoveNext();
                e.MoveNext();
                e.SkipToStructEnd();
                Assert.That(e.Current, Is.SameAs(frames[6]));
            }

            for (var i = 0; i < 8; i++)
                frames[i].Flags = FrameFlags.Default;

            frames[2].Flags = FrameFlags.BeginStruct;
            frames[3].Flags = FrameFlags.BeginStruct;
            frames[4].Flags = FrameFlags.EndStruct;
            frames[5].Flags = FrameFlags.EndStruct;

            using (var e = m.GetEnumerator())
            {
                e.MoveNext();
                while (!e.Current.IsBeginStruct) e.MoveNext();
                e.MoveNext();
                e.SkipToStructEnd();
                Assert.That(e.Current, Is.SameAs(frames[6]));
            }

            for (var i = 0; i < 8; i++)
                frames[i].Flags = FrameFlags.Default;

            using (var e = m.GetEnumerator())
            {
                e.MoveNext();
                e.MoveNext();
                Assert.Throws<InvalidOperationException>(() => e.SkipToStructEnd());
            }

            for (var i = 0; i < 8; i++)
                frames[i].Flags = FrameFlags.Default;

            frames[2].Flags = FrameFlags.Null;
            frames[3].Flags = FrameFlags.Null;
            frames[4].Flags = FrameFlags.Null;
            frames[5].Flags = FrameFlags.Null;
            frames[6].Flags = FrameFlags.Default;

            using (var e = m.GetEnumerator())
            {
                e.MoveNext();
                while (!e.Current.IsNull) e.MoveNext();
                Assert.That(e.SkipNull());
                Assert.That(e.SkipNull());
                Assert.That(e.SkipNull());
                Assert.That(e.SkipNull());
                Assert.That(e.SkipNull(), Is.False);
                Assert.That(e.Current, Is.SameAs(frames[6]));

                e.Reset();
                e.MoveNext();
                Assert.That(e.Current, Is.SameAs(frames[0]));
            }

            using (var e = m.GetEnumerator())
            {
                Console.WriteLine(e);
            }
        }

        [Test]
        public void Fragments()
        {
            ClientMessage m = null;
            Assert.Throws<ArgumentNullException>(() => _ = m.Fragment(512).GetEnumerator().MoveNext());

            // for some reason, fragment ids are unique
            ClientMessageFragmentingExtensions.FragmentIdSequence = new Int64Sequence();
            var fragmentIdSequence = 1L;

            // create 8 frames of *total* size 64 bytes
            var frames = new Frame[8];
            for (var i = 0; i < 8; i++)
                frames[i] = new Frame(new byte[64 - Hazelcast.Messaging.FrameFields.SizeOf.LengthAndFlags]);

            m = new ClientMessage();
            for (var i = 0; i < 8; i++)
                m.Append(frames[i]);
            Assert.That(m.LastFrame.IsFinal);

            void AssertFragments(List<ClientMessage> messages)
            {
                foreach (var message in messages)
                {
                    if (message == m)
                    {
                        // contains message = there should only be 1, with fragment id zero
                        Assert.That(message.FragmentId, Is.Zero);
                        continue;
                    }
                    Assert.That(message.FragmentId, Is.EqualTo(fragmentIdSequence++));
                }

                foreach (var message in messages)
                    Assert.That(message.LastFrame.Next, Is.Null);

                var frameCount = 1; // 'x' message 1st frame
                foreach (var fragment in messages)
                {
                    foreach (var frame in fragment)
                        frameCount++;
                }

                // beware, AppendFragment does *not* clone the fragments but MODIFIES them

                var x = new ClientMessage(new Frame());
                foreach (var fragment in messages)
                    x.AppendFragment(fragment.FirstFrame, fragment.LastFrame, false);
                Assert.That(x.Count(), Is.EqualTo(frameCount));
                Assert.That(x.LastFrame.IsFinal);
            }

            // frames are too big to even fit in 1 fragment = 8 fragments
            var fragments = m.Fragment(2).ToList();
            Assert.That(fragments.Count, Is.EqualTo(8));
            AssertFragments(fragments);

            // entire message fits in 1 fragment = 1 fragment
            fragments = m.Fragment(2048).ToList();
            Assert.That(fragments.Count, Is.EqualTo(1));
            AssertFragments(fragments);

            // each fragment can contain only 1 of the frames = 8 fragments
            fragments = m.Fragment(127).ToList();
            Assert.That(fragments.Count, Is.EqualTo(8));
            AssertFragments(fragments);

            // each fragment can contain exactly 2 frames = 4 fragments
            fragments = m.Fragment(128).ToList();
            Assert.That(fragments.Count, Is.EqualTo(4));
            AssertFragments(fragments);

            // each fragment can contain exactly 4 frames = 2 fragments
            fragments = m.Fragment(256).ToList();
            Assert.That(fragments.Count, Is.EqualTo(2));
            AssertFragments(fragments);

            // each fragment can contain exactly 8 frames = 1 fragments
            fragments = m.Fragment(512).ToList();
            Assert.That(fragments.Count, Is.EqualTo(1));
            AssertFragments(fragments);

            frames = new Frame[8];
            for (var i = 0; i < 8; i++)
                frames[i] = new Frame(new byte[(1 + i%2) * 64 - Hazelcast.Messaging.FrameFields.SizeOf.LengthAndFlags]);

            m = new ClientMessage();
            for (var i = 0; i < 8; i++)
                m.Append(frames[i]);

            fragments = m.Fragment(128).ToList();
            Assert.That(fragments.Count, Is.EqualTo(8));
            AssertFragments(fragments);

            frames = new Frame[8];
            for (var i = 0; i < 8; i++)
                frames[i] = new Frame(new byte[(1 + 2 * (i % 2)) * 64 - Hazelcast.Messaging.FrameFields.SizeOf.LengthAndFlags]);

            m = new ClientMessage();
            for (var i = 0; i < 8; i++)
                m.Append(frames[i]);

            fragments = m.Fragment(128).ToList();
            Assert.That(fragments.Count, Is.EqualTo(8));
            AssertFragments(fragments);

            Assert.Throws<ArgumentNullException>(() => _ = m.AppendFragment(null, null));
            Assert.Throws<ArgumentNullException>(() => _ = m.AppendFragment(new Frame(), null));
            Assert.Throws<ArgumentException>(() => _ = m.AppendFragment(new Frame(), new Frame()));
            Assert.Throws<InvalidOperationException>(() => _ = new ClientMessage().AppendFragment(new Frame(), new Frame()));
        }
    }
}