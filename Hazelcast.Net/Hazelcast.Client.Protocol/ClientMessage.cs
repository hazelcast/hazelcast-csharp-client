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
using System.Text;
using Hazelcast.IO;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol
{
    public class ClientMessage //, IOutboundFrame
    {
        // All offsets here are offset of frame.content byte[]
        // Note that frames have frame length and flags before this byte[] content
        public const int TypeFieldOffset = 0;
        public const int CorrelationIdFieldOffset = TypeFieldOffset + IntSizeInBytes;
        public const int ResponseBackupAcksFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;
        //offset valid for fragmentation frames only
        public const int FragmentationIdOffset = 0;
        //optional fixed partition id field offset
        public const int PartitionIdFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;

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
        public const int SizeOfFrameLengthAndFlags = IntSizeInBytes + ShortSizeInBytes;
        public static readonly Frame NullFrame = new Frame(new byte[0], IsNullFlag);
        public static readonly Frame BeginFrame = new Frame(new byte[0], BeginDataStructureFlag);
        public static readonly Frame EndFrame = new Frame(new byte[0], EndDataStructureFlag);

        private const long SerialVersionUid = 1L;

        public bool IsRetryable;
        public bool AcquiresResource;
        public string OperationName;

        Frame _startFrame;
        Frame _endFrame;

        private ClientMessage()
        {
        }

        private ClientMessage(Frame startFrame)
        {
            _startFrame = startFrame;
            _endFrame = startFrame;
            while (_endFrame.next != null)
            {
                _endFrame = _endFrame.next;
            }
        }

        public static ClientMessage CreateForEncode() => new ClientMessage();

        public static ClientMessage CreateForDecode(Frame frame)
        {
            var message = new ClientMessage();
            message.Add(frame);
            return message;
        }

        public Frame Head => _startFrame;
        public Frame Tail => _endFrame;

        public ClientMessage Add(Frame frame)
        {
            frame.next = null;
            if (_startFrame == null)
            {
                _startFrame = frame;
                _endFrame = frame;
                return this;
            }

            _endFrame.next = frame;
            _endFrame = frame;
            return this;
        }

        public void Merge(ClientMessage fragment)
        {
            // ignore the first frame of the fragment since first frame marks the fragment
            var fragmentMessageStartFrame = fragment._startFrame.next;
            _endFrame.next = fragmentMessageStartFrame;
            while (_endFrame.next != null)
            {
                _endFrame = _endFrame.next;
            }
        }

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
                int frameLength = 0;
                Frame currentFrame = _startFrame;
                while (currentFrame != null)
                {
                    frameLength += currentFrame.Size;
                    currentFrame = currentFrame.next;
                }
                return frameLength;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ClientMessage{");
            if (_startFrame != null)
            {
                sb.Append("length=").Append(FrameLength);
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
        public ClientMessage CopyWithNewCorrelationId(long correlationId)
        {
            var initialFrameCopy = _startFrame.DeepCopy();

            var newMessage = new ClientMessage(initialFrameCopy)
            {
                CorrelationId = correlationId,
                IsRetryable = IsRetryable,
                AcquiresResource = AcquiresResource,
                OperationName = OperationName
            };

            return newMessage;
        }

        public class Frame
        {
            public readonly byte[] Content;
            //begin-fragment end-fragment final begin-data-structure end-data-structure is-null is-event 9reserverd
            public int Flags;

            internal Frame next;

            public Frame(byte[] content): this(content, DefaultFlags)
            {
            }

            public Frame(byte[] content, int flags)
            {
                Content = content;
                Flags = flags;
            }

            // Shares the content bytes
            public Frame Copy()
            {
                return new Frame(Content, Flags)
                {
                    next = next
                };
            }

            // Copies the content bytes
            public Frame DeepCopy()
            {
                var newContent = new byte[Content.Length];
                Array.Copy(Content, newContent, Content.Length);

                return new Frame(newContent, Flags)
                {
                    next = next
                };
            }

            public bool IsEndFrame => IsFlagSet(Flags, EndDataStructureFlag);

            public bool IsBeginFrame => IsFlagSet(Flags, BeginDataStructureFlag);

            public bool IsNullFrame => IsFlagSet(Flags, IsNullFlag);

            public bool IsFinal => IsFlagSet(Flags, IsFinalFlag);

            public int Size => Content == null ? SizeOfFrameLengthAndFlags : SizeOfFrameLengthAndFlags + Content.Length;
        }

        public FrameIterator GetIterator() => new FrameIterator(_startFrame);

        public class FrameIterator
        {
            private Frame nextFrame;

            internal FrameIterator(Frame start)
            {
                nextFrame = start;
            }

            public Frame Next()
            {
                var result = nextFrame;
                if (nextFrame != null)
                {
                    nextFrame = nextFrame.next;
                }
                return result;
            }

            public bool HasNext => nextFrame != null;
            
            public Frame PeekNext() => nextFrame;
        }
    }
}