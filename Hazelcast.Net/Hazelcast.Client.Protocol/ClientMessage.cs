// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Text;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Protocol
{
    /// <summary>
    ///     Client Message is the carrier framed data as defined below.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         Client Message is the carrier framed data as defined below.
    ///     </p>
    ///     <p>
    ///         Any request parameter, response or event data will be carried in
    ///         the payload.
    ///     </p>
    ///     <p />
    ///     <pre>
    ///         0                   1                   2                   3
    ///         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///         |R|                      Frame Length                           |
    ///         +-------------+---------------+---------------------------------+
    ///         |  Version    |B|E|  Flags    |               Type              |
    ///         +-------------+---------------+---------------------------------+
    ///         |                                                               |
    ///         +                       CorrelationId                           +
    ///         |                                                               |
    ///         +---------------------------------------------------------------+
    ///         |                        PartitionId                            |
    ///         +-----------------------------+---------------------------------+
    ///         |        Data Offset          |                                 |
    ///         +-----------------------------+                                 |
    ///         |                      Message Payload Data                    ...
    ///         |                                                              ...
    ///     </pre>
    /// </remarks>
    internal class ClientMessage : MessageFlyweight, ISocketWritable, ISocketReadable, IClientMessage
    {
        /// <summary>Current protocol version</summary>
        public const short Version = 0;

        /// <summary>Begin Flag</summary>
        public const short BeginFlag = unchecked(0x80);

        /// <summary>End Flag</summary>
        public const short EndFlag = unchecked(0x40);

        /// <summary>Begin and End Flags</summary>
        public const short BeginAndEndFlags = BeginFlag | EndFlag;

        /// <summary>Listener Event Flag</summary>
        public const short ListenerEventFlag = unchecked(0x01);

        private const int InitialBufferSize = 1024;
        private const int FrameLengthFieldOffset = 0;
        private const int VersionFieldOffset = FrameLengthFieldOffset + Bits.IntSizeInBytes;
        private const int FlagsFieldOffset = VersionFieldOffset + Bits.ByteSizeInBytes;
        private const int TypeFieldOffset = FlagsFieldOffset + Bits.ByteSizeInBytes;
        private const int CorrelationIdFieldOffset = TypeFieldOffset + Bits.ShortSizeInBytes;
        private const int PartitionIdFieldOffset = CorrelationIdFieldOffset + Bits.LongSizeInBytes;
        private const int DataOffsetFieldOffset = PartitionIdFieldOffset + Bits.IntSizeInBytes;

        /// <summary>ClientMessage Fixed Header size in bytes</summary>
        public const int HeaderSize = DataOffsetFieldOffset + Bits.ShortSizeInBytes;

        private bool _isRetryable;
        private int _writeOffset;

        /// <param name="flag">Check this flag to see if it is set.</param>
        /// <returns>true if the given flag is set, false otherwise.</returns>
        public virtual bool IsFlagSet(short flag)
        {
            var i = GetFlags() & flag;
            return i == flag;
        }

        /// <summary>Sets the flags field value.</summary>
        /// <param name="flags">The value to set in the flags field.</param>
        /// <returns>The ClientMessage with the new flags field value.</returns>
        public virtual IClientMessage AddFlag(short flags)
        {
            Uint8Put(FlagsFieldOffset, (short) (GetFlags() | flags));
            return this;
        }

        /// <summary>Returns the message type field.</summary>
        /// <returns>The message type field value.</returns>
        public virtual int GetMessageType()
        {
            return Uint16Get(TypeFieldOffset);
        }

        /// <summary>Returns the correlation id field.</summary>
        /// <returns>The correlation id field.</returns>
        public virtual long GetCorrelationId()
        {
            return Int64Get(CorrelationIdFieldOffset);
        }

        /// <summary>Sets the correlation id field.</summary>
        /// <param name="correlationId">The value to set in the correlation id field.</param>
        /// <returns>The ClientMessage with the new correlation id field value.</returns>
        public virtual IClientMessage SetCorrelationId(long correlationId)
        {
            Int64Set(CorrelationIdFieldOffset, correlationId);
            return this;
        }

        /// <summary>Returns the partition id field.</summary>
        /// <returns>The partition id field.</returns>
        public virtual int GetPartitionId()
        {
            return Int32Get(PartitionIdFieldOffset);
        }

        /// <summary>Sets the partition id field.</summary>
        /// <param name="partitionId">The value to set in the partitions id field.</param>
        /// <returns>The ClientMessage with the new partitions id field value.</returns>
        public virtual IClientMessage SetPartitionId(int partitionId)
        {
            Int32Set(PartitionIdFieldOffset, partitionId);
            return this;
        }

        public virtual bool IsRetryable()
        {
            return _isRetryable;
        }

        public virtual bool ReadFrom(ByteBuffer source)
        {
            var frameLength = 0;
            if (Buffer == null)
            {
                //init internal buffer
                var remaining = source.Remaining();
                if (remaining < Bits.IntSizeInBytes) {
                    //we don't have even the frame length ready
                    return false;
                }
                frameLength = Bits.ReadIntL(source.Array(), source.Position);
                if (frameLength < HeaderSize)
                {
                    throw new InvalidDataException("Client message frame length cannot be smaller than header size.");
                }
                Wrap(new SafeBuffer(new byte[frameLength]), 0);
            }
            frameLength = frameLength > 0 ? frameLength : GetFrameLength();
            Accumulate(source, frameLength - Index());
            return IsComplete();
        }

        public virtual bool WriteTo(ByteBuffer destination)
        {
            var byteArray = Buffer.ByteArray();
            var size = GetFrameLength();
            // the number of bytes that can be written to the bb.
            var bytesWritable = destination.Remaining();
            // the number of bytes that need to be written.
            var bytesNeeded = size - _writeOffset;
            int bytesWrite;
            bool done;
            if (bytesWritable >= bytesNeeded)
            {
                // All bytes for the value are available.
                bytesWrite = bytesNeeded;
                done = true;
            }
            else
            {
                // Not all bytes for the value are available. Write as much as is available.
                bytesWrite = bytesWritable;
                done = false;
            }
            destination.Put(byteArray, _writeOffset, bytesWrite);
            _writeOffset += bytesWrite;
            if (done)
            {
                //clear the write offset so that same client message can be resend if needed.
                _writeOffset = 0;
            }
            return done;
        }

        public virtual bool IsUrgent()
        {
            return false;
        }

        public static ClientMessage Create()
        {
            return new ClientMessage();
        }

        public static ClientMessage CreateForDecode(IClientProtocolBuffer buffer, int offset)
        {
            var clientMessage = new ClientMessage();
            clientMessage.WrapForDecode(buffer, offset);
            return clientMessage;
        }

        public static ClientMessage CreateForEncode(int initialCapacity)
        {
            initialCapacity = QuickMath.NextPowerOfTwo(initialCapacity);
            return CreateForEncode(new SafeBuffer(new byte[initialCapacity]), 0);
        }

        public static ClientMessage CreateForEncode(IClientProtocolBuffer buffer, int offset)
        {
            var clientMessage = new ClientMessage();
            clientMessage.WrapForEncode(buffer, offset);
            return clientMessage;
        }

        /// <summary>Returns the setDataOffset field.</summary>
        /// <returns>The setDataOffset type field value.</returns>
        public virtual int GetDataOffset()
        {
            return Uint16Get(DataOffsetFieldOffset);
        }

        /// <summary>Returns the flags field value.</summary>
        /// <returns>The flags field value.</returns>
        public virtual short GetFlags()
        {
            return Uint8Get(FlagsFieldOffset);
        }

        /// <summary>Returns the frame length field.</summary>
        /// <returns>The frame length field.</returns>
        public virtual int GetFrameLength()
        {
            return Int32Get(FrameLengthFieldOffset);
        }

        /// <summary>Returns the version field value.</summary>
        /// <returns>The version field value.</returns>
        public virtual short GetVersion()
        {
            return Uint8Get(VersionFieldOffset);
        }

        /// <summary>Checks the frame size and total data size to validate the message size.</summary>
        /// <returns>true if the message is constructed.</returns>
        public virtual bool IsComplete()
        {
            return (Index() >= HeaderSize) && (Index() == GetFrameLength());
        }

        /// <summary>Sets the dataOffset field.</summary>
        /// <param name="dataOffset">The value to set in the dataOffset field.</param>
        /// <returns>The ClientMessage with the new dataOffset field value.</returns>
        public virtual ClientMessage SetDataOffset(int dataOffset)
        {
            Uint16Put(DataOffsetFieldOffset, dataOffset);
            return this;
        }

        /// <summary>Sets the frame length field.</summary>
        /// <param name="length">The value to set in the frame length field.</param>
        /// <returns>The ClientMessage with the new frame length field value.</returns>
        public virtual ClientMessage SetFrameLength(int length)
        {
            Int32Set(FrameLengthFieldOffset, length);
            return this;
        }

        /// <summary>Sets the message type field.</summary>
        /// <param name="type">The value to set in the message type field.</param>
        /// <returns>The ClientMessage with the new message type field value.</returns>
        public virtual ClientMessage SetMessageType(int type)
        {
            Uint16Put(TypeFieldOffset, type);
            return this;
        }

        public virtual void SetRetryable(bool isRetryable)
        {
            _isRetryable = isRetryable;
        }

        /// <summary>Sets the version field value.</summary>
        /// <param name="version">The value to set in the version field.</param>
        /// <returns>The ClientMessage with the new version field value.</returns>
        public virtual ClientMessage SetVersion(short version)
        {
            Uint8Put(VersionFieldOffset, version);
            return this;
        }

        public override string ToString()
        {
            var len = Index();
            var sb = new StringBuilder("ClientMessage{");
            sb.Append("length=").Append(len);
            if (len >= HeaderSize)
            {
                sb.Append(", correlationId=").Append(GetCorrelationId());
                sb.Append(", messageType=").Append(GetMessageType().ToString("X"));
                sb.Append(", partitionId=").Append(GetPartitionId());
                sb.Append(", isComplete=").Append(IsComplete());
                sb.Append(", isRetryable=").Append(IsRetryable());
                sb.Append(", isEvent=").Append(IsFlagSet(ListenerEventFlag));
                sb.Append(", writeOffset=").Append(_writeOffset);
            }
            sb.Append('}');
            return sb.ToString();
        }

        public virtual ClientMessage UpdateFrameLength()
        {
            SetFrameLength(Index());
            return this;
        }

        protected internal virtual void WrapForDecode(IClientProtocolBuffer buffer, int offset)
        {
            EnsureHeaderSize(offset, buffer.Capacity());
            Wrap(buffer, offset);
            Index(GetDataOffset());
        }

        protected internal virtual void WrapForEncode(IClientProtocolBuffer buffer, int offset)
        {
            EnsureHeaderSize(offset, buffer.Capacity());
            Wrap(buffer, offset);
            SetDataOffset(HeaderSize);
            SetFrameLength(HeaderSize);
            Index(GetDataOffset());
            SetPartitionId(-1);
        }

        private int Accumulate(ByteBuffer byteBuffer, int length)
        {
            var remaining = byteBuffer.Remaining();
            var readLength = remaining < length ? remaining : length;
            if (readLength > 0)
            {
                Buffer.PutBytes(Index(), byteBuffer.Array(), byteBuffer.Position, readLength);
                byteBuffer.Position = byteBuffer.Position + readLength;
                Index(Index() + readLength);
                return readLength;
            }
            return 0;
        }


        private void EnsureHeaderSize(int offset, int length)
        {
            if (length - offset < HeaderSize)
            {
                throw new IndexOutOfRangeException("ClientMessage buffer must contain at least " + HeaderSize +
                                                   " bytes! length: " + length + ", offset: " + offset);
            }
        }
    }
}