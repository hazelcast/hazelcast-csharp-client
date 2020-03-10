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
using System.Threading;
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
        private int _readOffset = InitialOffset;
        private ClientMessage _message;

        public ClientMessage Message => _message;

        public void Reset()
        {
            Interlocked.Exchange(ref _readOffset, InitialOffset);
            Interlocked.Exchange(ref _message, null);
        }

        public bool ReadFrom(ByteBuffer src)
        {
            for (;;)
            {
                if (ReadFrame(src))
                {
                    if (_message.LastFrame.IsFinal)
                    {
                        return true;
                    }
                    _readOffset = InitialOffset;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool ReadFrame(ByteBuffer src)
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
                var frameLength = Bits.ReadIntL(src.Array(), src.Position);
                if (frameLength < ClientMessage.SizeOfFrameLengthAndFlags)
                {
                    throw new Exception(
                        $"The client message frame reported illegal length ({frameLength} bytes). Minimal length is the size of frame header ({ClientMessage.SizeOfFrameLengthAndFlags} bytes).");
                }
                src.Position += Bits.IntSizeInBytes;
                var flags = Bits.ReadShortL(src.Array(), src.Position) & IntMask;
                src.Position += Bits.ShortSizeInBytes;

                var size = frameLength - ClientMessage.SizeOfFrameLengthAndFlags;
                var bytes = new byte[size];
                var frame = new ClientMessage.Frame(bytes, flags);
                if (_message == null)
                {
                    _message = ClientMessage.CreateForDecode(frame);
                }
                else
                {
                    _message.Add(frame);
                }
                _readOffset = 0;
                if (size == 0)
                {
                    return true;
                }
            }

            var tail = _message.LastFrame;
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