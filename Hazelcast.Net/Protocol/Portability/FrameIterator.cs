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

using Hazelcast.Messaging;

namespace Hazelcast.Protocol.Portability
{
    public class FrameIterator
    {
        private Frame _nextFrame;

        public FrameIterator(ClientMessage message)
        {
            _nextFrame = message.FirstFrame;
        }

        public Frame Next()
        {
            var result = _nextFrame;
            _nextFrame = _nextFrame?.Next;
            return result;
        }

        public bool HasNext => _nextFrame != null;

        public Frame PeekNext() => _nextFrame;

        public bool NextFrameIsNullMoveNext()
        {
            var isNull = _nextFrame != null && _nextFrame.IsNull;
            if (isNull) Next();
            return isNull;
        }

        public bool NextFrameIsEndStruct => _nextFrame != null && _nextFrame.IsEndStruct;
    }
}