﻿// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using System.Text;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Util;

namespace Hazelcast.Client
{
    internal class Error
    {
        /// <summary>ClientMessageType of this message</summary>
        public const int Type = ResponseMessageConst.Exception;

        public readonly string CauseClassName;
        public readonly int? CauseErrorCode;
        public readonly string ClassName;
        public readonly int ErrorCode;
        public readonly string Message;
        public readonly StackTraceElement[] StackTrace;

        public Error(int errorCode, string className, string message, string causeClassName, int? causeErrorCode,
            StackTraceElement[] stackTrace)
        {
            CauseClassName = causeClassName;
            CauseErrorCode = causeErrorCode;
            ClassName = className;
            ErrorCode = errorCode;
            Message = message;
            StackTrace = stackTrace;
        }

        public Error(IClientMessage message)
        {
            ErrorCode = message.GetInt();
            ClassName = message.GetStringUtf8();
            var messageIsNull = message.GetBoolean();
            if (!messageIsNull)
            {
                Message = message.GetStringUtf8();
            }
            var stackTraceCount = message.GetInt();
            StackTrace = new StackTraceElement[stackTraceCount];
            for (var i = 0; i < stackTraceCount; i++)
            {
                StackTrace[i] = StackTraceElementCodec.Decode(message);
            }
            CauseErrorCode = message.GetInt();
            var causeClassNameIsNull = message.GetBoolean();
            if (!causeClassNameIsNull)
            {
                CauseClassName = message.GetStringUtf8();
            }
        }

        public static Error Decode(IClientMessage flyweight)
        {
            return new Error(flyweight);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("ClassName: {0} ({1}) : {2}", ClassName, CauseClassName, Message).AppendLine();
            sb.Append("STACKTRACE:").AppendLine();
            foreach (var stackTraceElement in StackTrace)
            {
                sb.Append(stackTraceElement).AppendLine();
            }
            return base.ToString();
        }
    }
}