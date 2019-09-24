// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Protocol
{
    internal class ClientMessage : LinkedList<ClientMessage.Frame> //, IOutboundFrame
    {
        // All offsets here are offset of frame.content byte[]
        // Note that frames have frame length and flags before this byte[] content
        public const int TypeFieldOffset = 0;
        public const int CorrelationIdFieldOffset = TypeFieldOffset + Bits.IntSizeInBytes;
        //offset valid for fragmentation frames only
        public const int FragmentationIdOffset = 0;
        //optional fixed partition id field offset
        public const int PartitionIdFieldOffset = CorrelationIdFieldOffset + Bits.LongSizeInBytes;

        public const int DefaultFlags = 0;
        public const int BeginFragmentFlag = 1 << 15;
        public const int EndFragmentFlag = 1 << 14;
        public const int UnfragmentedMessage = BeginFragmentFlag | EndFragmentFlag;
        public const int IsFinalFlag = 1 << 13;
        public const int BeginDataStructureFlag = 1 << 12;
        public const int EndDataStructureFlag = 1 << 11;
        public const int IsNullFlag = 1 << 10;
        public const int IsEventFlag = 1 << 9;

        //frame length + flags
        public const int SizeOfFrameLengthAndFlags = Bits.IntSizeInBytes + Bits.ShortSizeInBytes;
        public static readonly Frame NullFrame = new Frame(new byte[0], IsNullFlag);
        public static readonly Frame BeginFrame = new Frame(new byte[0], BeginDataStructureFlag);
        public static readonly Frame EndFrame = new Frame(new byte[0], EndDataStructureFlag);

        private const long SerialVersionUid = 1L;

        public bool IsRetryable;
        public bool AcquiresResource;
        public string OperationName;
        public Connection Connection;

        private ClientMessage()
        {
        }

        private ClientMessage(LinkedList<Frame> frames) :base (frames)
        {
        }

        public static ClientMessage CreateForEncode() => new ClientMessage();

        public static ClientMessage CreateForDecode(LinkedList<Frame> frames) => new ClientMessage(frames);

        private Frame Head => First.Value;

        public int HeaderFlags => Head.Flags;

        public int MessageType
        {
            get => Bits.ReadIntL(Head.Content, TypeFieldOffset);
            set => Bits.WriteIntL(Head.Content, TypeFieldOffset, value);
        }

        public long CorrelationId
        {
            get => Bits.ReadLongL(Head.Content, CorrelationIdFieldOffset);
            set => Bits.WriteLongL(Head.Content, CorrelationIdFieldOffset, value);
        }

        public int PartitionId
        {
            get => Bits.ReadIntL(Head.Content, PartitionIdFieldOffset);
            set => Bits.WriteIntL(Head.Content, PartitionIdFieldOffset, value);
        }

        public static bool IsFlagSet(int flags, int flagMask)
        {
            var i = flags & flagMask;
            return i == flagMask;
        }

        public int FrameLength
        {
            get
            {
                var frameLength = 0;

                foreach (var node in this)
                {
                    frameLength += node.Content.Length;
                }

                return frameLength;
            }
        }

        public bool IsUrgent => false;

        public override string ToString()
        {
            var sb = new StringBuilder("ClientMessage{");
            sb.Append("connection=").Append(Connection);
            if (Count > 0)
            {
                sb.Append(", length=").Append(FrameLength);
                sb.Append(", correlationId=").Append(CorrelationId);
                sb.Append(", operation=").Append(OperationName);
                sb.Append(", messageType=").Append(MessageType.ToString("X"));
                sb.Append(", isRetryable=").Append(IsRetryable);
                sb.Append(", isEvent=").Append(IsFlagSet(Head.Flags, IsEventFlag));
                sb.Append(", isFragmented=").Append(!IsFlagSet(Head.Flags, UnfragmentedMessage));
            }
            sb.Append('}');
            return sb.ToString();
        }

        /**
         * Copies the clientMessage efficiently with correlation id
         * Only initialFrame is duplicated, rest of the frames are shared
         *
         * @param correlationId new id
         * @return the copy message
         */
        public ClientMessage copyWithNewCorrelationId(long correlationId)
        {
            ClientMessage newMessage = new ClientMessage(this);

            Frame initialFrameCopy = newMessage.get(0).copy();
            newMessage.AddFirst(initialFrameCopy);

            newMessage.CorrelationId = correlationId;

            newMessage.IsRetryable = IsRetryable;
            newMessage.AcquiresResource = AcquiresResource;
            newMessage.OperationName = OperationName;

            return newMessage;
        }

        //    @Override
        //public boolean equals(Object o)
        //    {
        //        if (this == o)
        //        {
        //            return true;
        //        }
        //        if (o == null || getClass() != o.getClass())
        //        {
        //            return false;
        //        }
        //        if (!super.equals(o))
        //        {
        //            return false;
        //        }

        //        ClientMessage message = (ClientMessage)o;

        //        if (isRetryable != message.isRetryable)
        //        {
        //            return false;
        //        }
        //        if (AcquiresResource != message.AcquiresResource)
        //        {
        //            return false;
        //        }
        //        if (!Objects.equals(OperationName, message.OperationName))
        //        {
        //            return false;
        //        }
        //        return Objects.equals(Connection, message.Connection);
        //    }

        //    public override int GetHashCode()
        //    {
        //        var result = base.GetHashCode();
        //        result = 31 * result + (IsRetryable ? 1 : 0);
        //        result = 31 * result + (AcquiresResource ? 1 : 0);
        //        result = 31 * result + (OperationName != null ? OperationName.GetHashCode() : 0);
        //        result = 31 * result + (Connection != null ? Connection.hashCode() : 0);
        //        return result;
        //    }

        public sealed class Frame
        {
            public readonly byte[] Content;
            //begin-fragment end-fragment final begin-data-structure end-data-structure is-null is-event 9reserverd
            public int Flags;

            public Frame(byte[] content) : this(content, DefaultFlags)
            {
            }

            public Frame(byte[] content, int flags)
            {
                Content = content ?? throw new ArgumentNullException(nameof(content));
                Flags = flags;
            }

            //public Frame copy()
            //{

            //    byte[] newContent = Arrays.copyOf(content, content.length);
            //    return new Frame(newContent, flags);
            //}

            public bool IsEndFrame => IsFlagSet(Flags, EndDataStructureFlag);

            public bool IsBeginFrame => IsFlagSet(Flags, BeginDataStructureFlag);

            public bool IsNullFrame => IsFlagSet(Flags, IsNullFlag);

            public int Size => Content == null ? SizeOfFrameLengthAndFlags : SizeOfFrameLengthAndFlags + Content.Length;

            //    @Override
            //public boolean equals(Object o)
            //    {
            //        if (this == o)
            //        {
            //            return true;
            //        }
            //        if (o == null || getClass() != o.getClass())
            //        {
            //            return false;
            //        }

            //        Frame frame = (Frame)o;

            //        if (Flags != frame.Flags)
            //        {
            //            return false;
            //        }
            //        return Array.equals(Content, frame.Content);
            //    }

            //    @Override
            //public int hashCode()
            //    {
            //        int result = Arrays.hashCode(Content);
            //        result = 31 * result + Flags;
            //        return result;
            //    }
        }
    }
}