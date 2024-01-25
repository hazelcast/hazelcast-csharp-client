﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

// <auto-generated>
//   This code was generated by a tool.
//   Hazelcast Client Protocol Code Generator @0a5719d
//   https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class StackTraceElementCodec
    {
        private const int LineNumberFieldOffset = 0;
        private const int InitialFrameSize = LineNumberFieldOffset + BytesExtensions.SizeOfInt;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Exceptions.StackTraceElement stackTraceElement)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteIntL(LineNumberFieldOffset, stackTraceElement.LineNumber);
            clientMessage.Append(initialFrame);

            StringCodec.Encode(clientMessage, stackTraceElement.ClassName);
            StringCodec.Encode(clientMessage, stackTraceElement.MethodName);
            CodecUtil.EncodeNullable(clientMessage, stackTraceElement.FileName, StringCodec.Encode);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Exceptions.StackTraceElement Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var lineNumber = initialFrame.Bytes.ReadIntL(LineNumberFieldOffset);

            var className = StringCodec.Decode(iterator);
            var methodName = StringCodec.Decode(iterator);
            var fileName = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);

            iterator.SkipToStructEnd();
            return new Hazelcast.Exceptions.StackTraceElement(className, methodName, fileName, lineNumber);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
