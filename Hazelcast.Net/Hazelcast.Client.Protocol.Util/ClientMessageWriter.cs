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

using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Util
{
    internal class ClientMessageWriter
    {
        private const int LengthNotWrittenYet = -1;
        private ClientMessage.Frame _currentFrame;
        private int _writeOffset = LengthNotWrittenYet;

        public bool WriteTo(ByteBuffer dst, ClientMessage clientMessage)
        {
            if (_currentFrame == null)
            {
                _currentFrame = clientMessage.FirstFrame;
            }
            for (; ; )
            {
                var isLastFrame = _currentFrame.next == null;
                if (WriteFrame(dst, _currentFrame, isLastFrame))
                {
                    _writeOffset = LengthNotWrittenYet;
                    if (isLastFrame)
                    {
                        _currentFrame = null;
                        return true;
                    }
                    _currentFrame = _currentFrame.next;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool WriteFrame(ByteBuffer dst, ClientMessage.Frame frame, bool isLastFrame)
        {
            // the number of bytes that can be written to the bb
            var bytesWritable = dst.Remaining();
            var frameContentLength = frame.Content == null ? 0 : frame.Content.Length;

            //if write offset is -1 put the length and flags byte first
            if (_writeOffset == -1)
            {
                if (bytesWritable >= ClientMessage.SizeOfFrameLengthAndFlags)
                {
                    Bits.WriteIntL(dst.Array(), dst.Position, frameContentLength + ClientMessage.SizeOfFrameLengthAndFlags);
                    dst.Position += Bits.IntSizeInBytes;

                    if (isLastFrame)
                    {
                        Bits.WriteShortL(dst.Array(), dst.Position, (short)(frame.Flags | ClientMessage.IsFinalFlag));
                    }
                    else
                    {
                        Bits.WriteShortL(dst.Array(), dst.Position, (short)frame.Flags);
                    }
                    dst.Position += Bits.ShortSizeInBytes;
                    _writeOffset = 0;
                }
                else
                {
                    return false;
                }
            }

            bytesWritable = dst.Remaining();

            if (frame.Content == null)
            {
                return true;
            }

            // the number of bytes that need to be written
            var bytesNeeded = frameContentLength - _writeOffset;

            int bytesWrite;
            bool done;
            if (bytesWritable >= bytesNeeded)
            {
                // all bytes for the value are available
                bytesWrite = bytesNeeded;
                done = true;
            }
            else
            {
                // not all bytes for the value are available. Write as much as is available
                bytesWrite = bytesWritable;
                done = false;
            }

            dst.Put(frame.Content, _writeOffset, bytesWrite);
            _writeOffset += bytesWrite;

            return done;
        }
    }
}