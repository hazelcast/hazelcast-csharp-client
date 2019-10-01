/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

//package com.hazelcast.client.impl.protocol.codec.builtin;

//import com.hazelcast.client.impl.protocol.ClientMessage;

//import java.util.ListIterator;
//import java.util.function.BiConsumer;
//import java.util.function.Function;

//import static com.hazelcast.client.impl.protocol.ClientMessage.NULL_FRAME;

using System;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    delegate T DecodeDelegate<T>(ref ClientMessage.FrameIterator iterator);

    internal static class CodecUtil
    {
        public static void FastForwardToEndFrame(this ref ClientMessage.FrameIterator iterator)
        {
            // We are starting from 1 because of the BeginFrame we read
            // in the beginning of the Decode method
            var numberOfExpectedEndFrames = 1;

            while (numberOfExpectedEndFrames != 0)
            {
                var frame = iterator.Next();
                if (frame.IsEndFrame)
                {
                    numberOfExpectedEndFrames--;
                }
                else if (frame.IsBeginFrame)
                {
                    numberOfExpectedEndFrames++;
                }
            }
        }

        public static void EncodeNullable<T>(ClientMessage clientMessage, T value, Action<ClientMessage, T> encode)
        {
            if (value == null)
            {
                clientMessage.Add(ClientMessage.NullFrame);
            }
            else
            {
                encode(clientMessage, value);
            }
        }

        public static T DecodeNullable<T>(ref ClientMessage.FrameIterator iterator, DecodeDelegate<T> decode)
        {
            return IsNextFrameIsNullEndFrame(ref iterator) ? default : decode(ref iterator);
        }

        public static bool IsNextFrameIsDataStructureEndFrame(this ref ClientMessage.FrameIterator iterator)
        {
            try
            {
                return iterator.Next().IsEndFrame;
            }
            finally
            {
                iterator.Previous();
            }
        }

        public static bool IsNextFrameIsNullEndFrame(this ref ClientMessage.FrameIterator iterator)
        {
            var isNull = iterator.Next().IsNullFrame;
            if (!isNull)
            {
                iterator.Previous();
            }
            return isNull;
        }
    }
}