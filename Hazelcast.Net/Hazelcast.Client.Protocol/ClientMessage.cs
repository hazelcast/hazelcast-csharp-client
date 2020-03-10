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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol
{
    [DebuggerDisplay("")]
    internal class ClientMessage
    {
        // All offsets here are offset of frame.content byte[]
        // Note that frames have frame length and flags before this byte[] content
        public const int TypeFieldOffset = 0;
        public const int CorrelationIdFieldOffset = TypeFieldOffset + IntSizeInBytes;
        //backup acks field offset is used by response messages
        public const int ResponseBackupAcksFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;
        //partition id field offset used by request and event messages
        public const int PartitionIdFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;

        //offset valid for fragmentation frames only
        public const int FragmentationIdOffset = 0;

        public const int DefaultFlags = 0;
        public const int BeginFragmentFlag = 1 << 15;
        public const int EndFragmentFlag = 1 << 14;
        public const int UnfragmentedMessage = BeginFragmentFlag | EndFragmentFlag;
        public const int IsFinalFlag = 1 << 13;
        public const int BeginDataStructureFlag = 1 << 12;
        public const int EndDataStructureFlag = 1 << 11;
        public const int IsNullFlag = 1 << 10;
        public const int IsEventFlag = 1 << 9;
        public const int BackupAwareFlag = 1 << 8;
        public const int BackupEventFlag = 1 << 7;

        //frame length + flags
        public const int SizeOfFrameLengthAndFlags = IntSizeInBytes + ShortSizeInBytes;
        public static readonly Frame NullFrame = new Frame(new byte[0], IsNullFlag);
        public static readonly Frame BeginFrame = new Frame(new byte[0], BeginDataStructureFlag);
        public static readonly Frame EndFrame = new Frame(new byte[0], EndDataStructureFlag);

        public bool IsRetryable;
        public string OperationName;
        public Frame FirstFrame { get; private set; }
        public Frame LastFrame { get; private set; }

        public int HeaderFlags => FirstFrame.Flags;

        private ClientMessage()
        {
        }

        //Constructs client message with single frame. StartFrame.next must be null.
        private ClientMessage(Frame firstFrame)
        {
            Debug.Assert(firstFrame.next == null);
            FirstFrame = firstFrame;
            LastFrame = firstFrame;
        }
        
        private ClientMessage(Frame firstFrame, Frame lastFrame)
        {
            FirstFrame = firstFrame;
            LastFrame = lastFrame;
        }

        public static ClientMessage CreateForEncode() => new ClientMessage();

        public static ClientMessage CreateForDecode(Frame startFrame) => new ClientMessage(startFrame);

        public void Add(Frame frame)
        {
            frame.next = null;
            if (FirstFrame == null)
            {
                FirstFrame = frame;
                LastFrame = frame;
            }
            else
            {
                LastFrame.next = frame;
                LastFrame = frame;
            }
        }

        public void Merge(ClientMessage fragment)
        {
            // ignore the first frame of the fragment since first frame marks the fragment
            LastFrame.next = fragment.FirstFrame.next;
            LastFrame = fragment.LastFrame;
        }

        public int MessageType
        {
            get => ReadIntL(FirstFrame.Content, TypeFieldOffset);
            set => WriteIntL(FirstFrame.Content, TypeFieldOffset, value);
        }

        public long CorrelationId
        {
            get => ReadLongL(FirstFrame.Content, CorrelationIdFieldOffset);
            set => WriteLongL(FirstFrame.Content, CorrelationIdFieldOffset, value);
        }

        public int PartitionId
        {
            get => ReadIntL(FirstFrame.Content, PartitionIdFieldOffset);
            set => WriteIntL(FirstFrame.Content, PartitionIdFieldOffset, value);
        }

        public static bool IsFlagSet(int flags, int flagMask)
        {
            var i = flags & flagMask;
            return i == flagMask;
        }

        public bool IsBackupAware => IsFlagSet(HeaderFlags, BackupAwareFlag);

        public bool IsBackupEvent => IsFlagSet(HeaderFlags, BackupEventFlag);

        public bool IsEvent => IsFlagSet(HeaderFlags, IsEventFlag);

        public bool IsExceptionType => MessageType == ErrorsCodec.ExceptionMessageType;


        // public int FrameLength
        // {
        //     get
        //     {
        //         var frameLength = 0;
        //         var currentFrame = FirstFrame;
        //         while (currentFrame != null)
        //         {
        //             frameLength += currentFrame.Size;
        //             currentFrame = currentFrame.next;
        //         }
        //         return frameLength;
        //     }
        // }

        public ClientMessage CopyWithNewCorrelationId(long correlationId)
        {
            var initialFrameCopy = FirstFrame.DeepCopy();
            var newMessage = new ClientMessage(initialFrameCopy, LastFrame)
            {
                CorrelationId = correlationId, IsRetryable = IsRetryable, OperationName = OperationName
            };
            return newMessage;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ClientMessage{");
            if (FirstFrame != null)
            {
                // sb.Append("length=").Append(FrameLength);
                sb.Append(", correlationId=").Append(CorrelationId);
                sb.Append(", operation=").Append(OperationName);
                sb.Append(", messageType=0x").Append(MessageType.ToString("X")).Append("(").Append(MessageType).Append(")");
                sb.Append(", isRetryable=").Append(IsRetryable);
                sb.Append(", isEvent=").Append(IsFlagSet(FirstFrame.Flags, IsEventFlag));
                sb.Append(", isFragmented=").Append(!IsFlagSet(FirstFrame.Flags, UnfragmentedMessage));
            }
            sb.Append('}');
            return sb.ToString();
        }

        public FrameIterator GetIterator() => new FrameIterator(FirstFrame);

        internal class Frame
        {
            public readonly byte[] Content;

            //begin-fragment end-fragment final begin-data-structure end-data-structure is-null is-event 9reserved
            public readonly int Flags;

            internal Frame next;

            public Frame(byte[] content, int flags = DefaultFlags)
            {
                Content = content;
                Flags = flags;
            }

            // Shares the content bytes
            public Frame Copy()
            {
                return new Frame(Content, Flags) {next = next};
            }

            // Copies the content bytes
            public Frame DeepCopy()
            {
                var newContent = new byte[Content.Length];
                Buffer.BlockCopy(Content, 0, newContent, 0, Content.Length);
                return new Frame(newContent, Flags) {next = next};
            }

            public bool IsEndFrame => IsFlagSet(Flags, EndDataStructureFlag);

            public bool IsBeginFrame => IsFlagSet(Flags, BeginDataStructureFlag);

            public bool IsNullFrame => IsFlagSet(Flags, IsNullFlag);

            public bool IsFinal => IsFlagSet(Flags, IsFinalFlag);

            public int Size => Content == null ? SizeOfFrameLengthAndFlags : SizeOfFrameLengthAndFlags + Content.Length;

            protected bool Equals(Frame other)
            {
                return Content.SequenceEqual(other.Content) && Flags == other.Flags;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Frame) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Content != null ? Content.GetHashCode() : 0) * 397) ^ Flags;
                }
            }
        }

        public class FrameIterator
        {
            private Frame _nextFrame;

            internal FrameIterator(Frame start)
            {
                _nextFrame = start;
            }

            public Frame Next()
            {
                var result = _nextFrame;
                if (_nextFrame != null)
                {
                    _nextFrame = _nextFrame.next;
                }
                return result;
            }

            public bool HasNext => _nextFrame != null;

            public Frame PeekNext() => _nextFrame;
        }
    }
}