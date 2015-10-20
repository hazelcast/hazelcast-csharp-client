/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

﻿using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class StackTraceElementCodec
    {
        private StackTraceElementCodec()
        {
        }

        public static int CalculateDataSize(StackTraceElement stackTraceElement)
        {
            var dataSize = Bits.IntSizeInBytes;
            dataSize += ParameterUtil.CalculateDataSize(stackTraceElement.ClassName);
            dataSize += ParameterUtil.CalculateDataSize(stackTraceElement.MethodName);
            dataSize += Bits.BooleanSizeInBytes;
            string fileName = stackTraceElement.FileName;
            var fileName_NotNull = fileName != null;
            if (fileName_NotNull)
            {
                dataSize += ParameterUtil.CalculateDataSize(fileName);
            }
            return dataSize;
        }

        public static StackTraceElement Decode(IClientMessage clientMessage)
        {
            var declaringClass = clientMessage.GetStringUtf8();
            var methodName = clientMessage.GetStringUtf8();
            var fileName_notNull = clientMessage.GetBoolean();
            string fileName = null;
            if (fileName_notNull)
            {
                fileName = clientMessage.GetStringUtf8();
            }
            var lineNumber = clientMessage.GetInt();
            return new StackTraceElement(declaringClass, methodName, fileName, lineNumber);
        }

        public static void Encode(StackTraceElement stackTraceElement, ClientMessage clientMessage)
        {
            clientMessage.Set(stackTraceElement.ClassName);
            clientMessage.Set(stackTraceElement.MethodName);
            string fileName = stackTraceElement.FileName;
            var fileName_NotNull = fileName != null;
            clientMessage.Set(fileName_NotNull);
            if (fileName_NotNull)
            {
                clientMessage.Set(fileName);
            }
            clientMessage.Set(stackTraceElement.LineNumber);
        }
    }
}