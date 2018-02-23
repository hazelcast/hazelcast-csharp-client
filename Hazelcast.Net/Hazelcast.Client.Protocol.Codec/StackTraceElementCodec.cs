// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Util;

namespace Hazelcast.Client.Protocol.Codec
{
    internal static class StackTraceElementCodec
    {
        public static StackTraceElement Decode(IClientMessage clientMessage)
        {
            var declaringClass = clientMessage.GetStringUtf8();
            var methodName = clientMessage.GetStringUtf8();
            var filename_Null = clientMessage.GetBoolean();
            string fileName = null;
            if (!filename_Null)
            {
                fileName = clientMessage.GetStringUtf8();
            }
            var lineNumber = clientMessage.GetInt();
            return new StackTraceElement(declaringClass, methodName, fileName, lineNumber);
        }
    }
}