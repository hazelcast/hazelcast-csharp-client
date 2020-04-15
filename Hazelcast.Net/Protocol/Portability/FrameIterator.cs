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
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.Portability
{
    public class FrameIterator
    {
        private Frame _frame;

        public FrameIterator(ClientMessage message)
        {
            _frame = message.FirstFrame;
        }

        public Frame Take()
        {
            var result = _frame;
            _frame = _frame?.Next;
            return result;
        }

        public bool HasCurrent => _frame != null;

        public Frame Peek() => _frame;

        public bool SkipNull()
        {
            var isNull = _frame != null && _frame.IsNull;
            if (isNull) Take();
            return isNull;
        }

        public bool CurrentIsEndStruct => _frame != null && _frame.IsEndStruct;

        public void SkipToStructEnd()
        {
            // We are starting from 1 because of the BeginFrame we read
            // in the beginning of the Decode method
            var numberOfExpectedEndFrames = 1;

            while (numberOfExpectedEndFrames != 0)
            {
                var frame = Take();
                if (frame == null)
                    throw new InvalidOperationException("Reached end of message.");

                if (frame.IsEndStruct)
                {
                    numberOfExpectedEndFrames--;
                }
                else if (frame.IsBeginStruct)
                {
                    numberOfExpectedEndFrames++;
                }
            }
        }


    }
}