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

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class CodecUtil
    {

        public static void fastForwardToEndFrame(ListIterator<ClientMessage.Frame> iterator)
        {
            // We are starting from 1 because of the BeginFrame we read
            // in the beginning of the Decode method
            int numberOfExpectedEndFrames = 1;
            ClientMessage.Frame frame;
            while (numberOfExpectedEndFrames != 0)
            {
                frame = iterator.next();
                if (frame.isEndFrame())
                {
                    numberOfExpectedEndFrames--;
                }
                else if (frame.isBeginFrame())
                {
                    numberOfExpectedEndFrames++;
                }
            }
        }

        public static <T> void EncodeNullable(ClientMessage clientMessage, T value, BiConsumer<ClientMessage, T> Encode)
        {
            if (value == null)
            {
                clientMessage.add(NULL_FRAME);
            }
            else
            {
                Encode.accept(clientMessage, value);
            }
        }

        public static <T> T DecodeNullable(ListIterator<ClientMessage.Frame> iterator,
            Function<ListIterator<ClientMessage.Frame>, T> Decode)
        {
            return nextFrameIsNullEndFrame(iterator) ? null : Decode.apply(iterator);
        }

        public static boolean nextFrameIsDataStructureEndFrame(ListIterator<ClientMessage.Frame> iterator)
        {
            try
            {
                return iterator.next().isEndFrame();
            }
            finally
            {
                iterator.previous();
            }
        }

        public static boolean nextFrameIsNullEndFrame(ListIterator<ClientMessage.Frame> iterator)
        {
            boolean isNull = iterator.next().isNullFrame();
            if (!isNull)
            {
                iterator.previous();
            }

            return isNull;
        }
    }
}