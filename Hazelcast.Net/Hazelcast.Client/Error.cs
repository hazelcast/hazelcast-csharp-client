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
            var message_isNull = message.GetBoolean();
            if (!message_isNull)
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
            var causeClassName_isNull = message.GetBoolean();
            if (!causeClassName_isNull)
            {
                CauseClassName = message.GetStringUtf8();
            }
        }

        public static Error Decode(IClientMessage flyweight)
        {
            return new Error(flyweight);
        }
    }
}