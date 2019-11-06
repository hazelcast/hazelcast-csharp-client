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
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Util
{
    /// <summary>
    /// Builds <see cref="Hazelcast.Client.Protocol.ClientMessage" />s from byte chunks. Fragmented messages are merged into single messages before processed.
    /// </summary>
    internal class ClientMessageReader
    {
        private const int IntMask = 0xffff;
        private const int InitialOffset = -1;

        private readonly int _maxMessageLength;

        private int _readOffset = InitialOffset;
        private int _sumUntrustedMessageLength;

        public ClientMessageReader(int maxMessageLength = int.MaxValue)
        {
            _maxMessageLength = maxMessageLength;
        }

        public ClientMessage Message { get; private set; }

        public bool ReadFrom(ByteBuffer src, bool trusted)
        {
            while (ReadFrame(src, trusted))
            {
                if (Message.Tail.IsEndFrame)
                {
                    return true;
                }
                _readOffset = InitialOffset;
            }

            return false;
        }

        private bool ReadFrame(ByteBuffer src, bool trusted)
        {
            // init internal buffer
            var remaining = src.Remaining();

            if (remaining < ClientMessage.SizeOfFrameLengthAndFlags)
            {
                // we don't have even the frame length and flags ready
                return false;
            }

            if (_readOffset == InitialOffset)
            {
                int frameLength = Bits.ReadIntL(src.Array(), src.Position);
                if (frameLength < ClientMessage.SizeOfFrameLengthAndFlags)
                {
                    throw new Exception($"The client message frame reported illegal length ({frameLength} bytes). Minimal length is the size of frame header ({ClientMessage.SizeOfFrameLengthAndFlags} bytes).");
                }

                if (!trusted)
                {
                    // check the message size overflow and message size limit
                    if (int.MaxValue - frameLength < _sumUntrustedMessageLength
                            || _sumUntrustedMessageLength + frameLength > _maxMessageLength)
                    {
                        throw new Exception($"The client message size ({_sumUntrustedMessageLength} + {frameLength}) exceeds the maximum allowed length ({_maxMessageLength})");
                    }
                    _sumUntrustedMessageLength += frameLength;
                }

                src.Position += Bits.IntSizeInBytes;
                var flags = Bits.ReadShortL(src.Array(), src.Position) & IntMask;
                src.Position += Bits.ShortSizeInBytes;

                var size = frameLength - ClientMessage.SizeOfFrameLengthAndFlags;
                var bytes = new byte[size];
                var frame = new ClientMessage.Frame(bytes, flags);
                if (Message == null)
                {
                    Message = ClientMessage.CreateForDecode(frame);
                }
                else
                {
                    Message.Add(frame);
                }
                _readOffset = 0;
                if (size == 0)
                {
                    return true;
                }
            }

            var tail = Message.Tail;
            return Accumulate(src, tail.Content, tail.Content.Length - _readOffset);
        }

        private bool Accumulate(ByteBuffer src, byte[] dest, int length)
        {
            var remaining = src.Remaining();
            var readLength = remaining < length ? remaining : length;
            if (readLength > 0)
            {
                src.Get(dest, _readOffset, readLength);
                _readOffset += readLength;
                return readLength == length;
            }

            return false;
        }
    }
}