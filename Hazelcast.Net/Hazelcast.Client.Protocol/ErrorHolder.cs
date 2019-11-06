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
using System.Collections.Generic;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Util;

namespace Hazelcast.Client.Protocol
{
    internal class ErrorHolder
    {
        public int ErrorCode { get; }
        public string ClassName { get; }
        public string Message { get; }
        public IEnumerable<StackTraceElement> StackTraceElements { get; }

        public ErrorHolder(int errorCode, string className, string message, IEnumerable<StackTraceElement> stackTraceElements)
        {
            ErrorCode = errorCode;
            ClassName = className;
            Message = message;
            StackTraceElements = stackTraceElements;
        }

        public static ErrorHolder Decode(ClientMessage message)
        {
            var iterator = message.GetIterator();
            return ErrorHolderCodec.Decode(ref iterator);
        }
    }
}