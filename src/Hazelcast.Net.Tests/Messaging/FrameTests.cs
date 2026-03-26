// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Messaging;
using NUnit.Framework;

namespace Hazelcast.Tests.Messaging
{
    [TestFixture]
    public class FrameTests
    {
        [Test]
        public void Properties()
        {
            var f = new Frame();
            Assert.That(f.Flags, Is.EqualTo(FrameFlags.Default));

            f.Flags = FrameFlags.BeginStruct;
            Assert.That(f.IsBeginStruct);

            f.Flags = FrameFlags.EndStruct;
            Assert.That(f.IsEndStruct);

            Console.WriteLine(f.ToString());
        }

        [Test]
        public void MemoryCtor_StoresBytesCorrectly()
        {
            var bytes = new byte[] { 1, 2, 3, 4 };
            var f = new Frame(new Memory<byte>(bytes));

            Assert.That(f.Bytes.Length, Is.EqualTo(4));
            Assert.That(f.Bytes.ToArray(), Is.EqualTo(bytes));
        }

        [Test]
        public void ReadOnlyMemoryCtor_StoresBytesAndOwner()
        {
            var bytes = new byte[] { 10, 20, 30 };
            var owner = new TrackingDisposable();
            var f = new Frame(new ReadOnlyMemory<byte>(bytes), FrameFlags.Default, owner);

            Assert.That(f.Bytes.Length, Is.EqualTo(3));
            Assert.That(f.Owner, Is.SameAs(owner));
        }

        [Test]
        public void Dispose_CallsOwnerDispose()
        {
            var owner = new TrackingDisposable();
            var f = new Frame(new ReadOnlyMemory<byte>(new byte[4]), FrameFlags.Default, owner);

            f.Dispose();

            Assert.That(owner.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_IsIdempotent_OwnerDisposedOnlyOnce()
        {
            var owner = new TrackingDisposable();
            var f = new Frame(new ReadOnlyMemory<byte>(new byte[4]), FrameFlags.Default, owner);

            f.Dispose();
            f.Dispose();
            f.Dispose();

            Assert.That(owner.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_WithNullOwner_DoesNotThrow()
        {
            var f = new Frame(new byte[] { 1, 2, 3 });
            Assert.DoesNotThrow(() => f.Dispose());
        }

        [Test]
        public void Length_IncludesHeaderAndBytes()
        {
            // Frame.Length = 4 (length field) + 2 (flags field) + bytes.Length
            var f = new Frame(new byte[10]);
            Assert.That(f.Length, Is.EqualTo(6 + 10));
        }

        [Test]
        public void DeepClone_ProducesIndependentCopy()
        {
            var bytes = new byte[] { 1, 2, 3, 4 };
            var f = new Frame(bytes);
            var clone = f.DeepClone();

            Assert.That(clone.Bytes.ToArray(), Is.EqualTo(bytes));
            // modifying original should not affect clone
            bytes[0] = 99;
            Assert.That(clone.Bytes.Span[0], Is.EqualTo(1));
        }

        private sealed class TrackingDisposable : IDisposable
        {
            public int DisposeCount;
            public void Dispose() => DisposeCount++;
        }
    }
}
