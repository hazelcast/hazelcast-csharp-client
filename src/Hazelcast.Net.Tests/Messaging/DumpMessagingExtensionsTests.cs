// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol.Codecs;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Messaging
{
    [TestFixture]
    public class DumpMessagingExtensionsTests
    {
        [Test]
        public void DumpMessage()
        {
            Assert.Throws<ArgumentNullException>(() => _ = ((ClientMessage) null).Dump(0));

            // exception
            var m = new ClientMessage(new Frame(new byte[64]));
            m.MessageType = 0;
            var s = m.Dump(int.MaxValue);

#if DEBUG
            Assert.That(s.ToLf(), Is.EqualTo(@"EXCEPTION
FRAME {Frame: 70 bytes, Final (0x00002000)}".ToLf()));
#else
            // works, but produces an empty string
            Assert.That(s.ToLf(), Is.EqualTo(""));
#endif
            // message
            m = new ClientMessage(new Frame(new byte[64]));
            m.Append(new Frame(new byte[64]));
            m.Append(new Frame(new byte[64]));
            m.MessageType = MapAddEntryListenerCodec.RequestMessageType;
            m.Flags |= ClientMessageFlags.Unfragmented;
            m.PartitionId = 55;
            m.OperationName = "operation";
            s = m.Dump(int.MaxValue);

#if DEBUG
            Assert.That(s.ToLf(), Is.EqualTo(@"REQUEST [0]
TYPE 0x11900 operation
PARTID 55
FRAME {Frame: 70 bytes, Unfragmented (0x0000C000)}
FRAME {Frame: 70 bytes, Default (0x00000000)}
FRAME {Frame: 70 bytes, Final (0x00002000)}".ToLf()));
#else
            // works, but produces an empty string
            Assert.That(s.ToLf(), Is.EqualTo(""));
#endif
            m = new ClientMessage(new Frame(new byte[64]));
            m.Append(new Frame(new byte[64]));
            m.Append(new Frame(new byte[64]));
            m.MessageType = MapAddEntryListenerCodec.ResponseMessageType;
            m.Flags |= ClientMessageFlags.Unfragmented;
            s = m.Dump(int.MaxValue);

#if DEBUG
            Assert.That(s.ToLf(), Is.EqualTo(@"RESPONSE [0]
TYPE 0x11901 MapAddEntryListener.Response
FRAME {Frame: 70 bytes, Unfragmented (0x0000C000)}
FRAME {Frame: 70 bytes, Default (0x00000000)}
FRAME {Frame: 70 bytes, Final (0x00002000)}".ToLf()));
#else
            // works, but produces an empty string
            Assert.That(s.ToLf(), Is.EqualTo(""));
#endif

            // event
            m = new ClientMessage(new Frame(new byte[64]));
            m.MessageType = 0x011902; // MapAddEntryListenerCodec.EventEntryMessageType;
            m.Flags |= ClientMessageFlags.Event;
            s = m.Dump(int.MaxValue);

#if DEBUG
            Assert.That(s.ToLf(), Is.EqualTo(@"EVENT [0]
TYPE 0x11902 MapAddEntryListener.Entry
FRAME {Frame: 70 bytes, Final, Event (0x00002200)}".ToLf()));
#else
            // works, but produces an empty string
            Assert.That(s.ToLf(), Is.EqualTo(""));
#endif

        }
    }
}
