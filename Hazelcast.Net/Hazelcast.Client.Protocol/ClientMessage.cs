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

        private Frame _head;
        private Frame[] _tail;
        private int _written;

        private ClientMessage()
        {
        }

        private ClientMessage(ClientMessage message)
        {
            _head = message._head;
            _tail = message._tail;
            _written = message._written;
        }

        public static ClientMessage CreateForEncode() => new ClientMessage();

        public static ClientMessage CreateForDecode(Frame frame)
        {
            var message = new ClientMessage();
            message.Add(frame);
            return message;
        }

        public ref Frame Head => ref Get(0);

        public ref Frame Tail => ref Get(_written - 1);

        public void Add(Frame frame)
        {
            switch (_written)
            {
                case 0:
                    _head = frame;
                    break;
                case 1:
                    _tail = new Frame[1];
                    _tail[0] = frame;
                    break;
                default:
                    var index = _written - 1;
                    if (index >= _tail.Length)
                    {
                        Array.Resize(ref _tail, _tail.Length * 2);
                    }

                    _tail[index] = frame;
                    break;
            }

            _written += 1;
        }

        public ref Frame Get(int index)
        {
            if (index == 0 && _written > 0)
            {
                return ref _head;
            }

            return ref _tail[index - 1];
        }

        public int FrameCount => _written;

        public void Merge(ClientMessage fragment)
        {
            // ignore the first frame of the fragment since first frame marks the fragment
            var count = fragment._written;
            for (var i = 1; i < count; i++)
            {
                Add(fragment.Get(i));
            }
        }

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

                for (var i = 0; i < _written; i++)
                {
                    frameLength += Get(i).Content.Length;
                }

                return frameLength;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ClientMessage{");
            if (_written > 0)
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
            var newMessage = new ClientMessage(this);

            // replace the first frame, deep copy
            var initialFrameCopy = newMessage.Head.Copy();
            newMessage._head = initialFrameCopy;

            newMessage.CorrelationId = correlationId;

            newMessage.IsRetryable = IsRetryable;
            newMessage.AcquiresResource = AcquiresResource;
            newMessage.OperationName = OperationName;

            return newMessage;
        }

        public struct Frame
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

            public Frame Copy()
            {
                var bytes = new byte[Content.Length];
                Buffer.BlockCopy(Content, 0, bytes, 0, Content.Length);
                return new Frame(bytes, Flags);
            }

            public bool IsEndFrame => IsFlagSet(Flags, EndDataStructureFlag);

            public bool IsBeginFrame => IsFlagSet(Flags, BeginDataStructureFlag);

            public bool IsNullFrame => IsFlagSet(Flags, IsNullFlag);

            public int Size => Content == null ? SizeOfFrameLengthAndFlags : SizeOfFrameLengthAndFlags + Content.Length;
        }

        public FrameIterator GetIterator() => new FrameIterator(this);

        public ref struct FrameIterator
        {
            private readonly ClientMessage _message;
            private int _index;

            internal FrameIterator(ClientMessage message)
            {
                _message = message;
                _index = -1;
            }

            public ref Frame Next()
            {
                _index += 1;
                return ref _message.Get(_index);
            }

            public ref Frame Previous()
            {
                _index -= 1;
                return ref _message.Get(_index);
            }
        }

        public struct Accessor
        {
            readonly ClientMessage _message;
            int _index;

            public Accessor(ClientMessage message)
            {
                _message = message;
                _index = 0;
            }

            public bool IsEmpty => _message == null || _index >= _message._written;
            public bool IsLast => _index == _message._written - 1;
            public ref Frame Frame => ref _message.Get(_index);
            public void MoveNext() => _index++;
        }

        public Accessor GetAccessor() => new Accessor(this);
    }
}