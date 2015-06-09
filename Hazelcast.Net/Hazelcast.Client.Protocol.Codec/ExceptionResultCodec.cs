using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec
{
	internal class ExceptionResultCodec
	{
		public const int Type = ResponseMessageConst.Exception;

		public string className;

		public string causeClassName;

		public string message;

		public string stacktrace;

		private ExceptionResultCodec(ClientMessage flyweight)
		{
			className = flyweight.GetStringUtf8();
			bool causeClassName_isNull = flyweight.GetBoolean();
			if (!causeClassName_isNull)
			{
				causeClassName = flyweight.GetStringUtf8();
			}
			bool message_isNull = flyweight.GetBoolean();
			if (!message_isNull)
			{
				message = flyweight.GetStringUtf8();
			}
			bool stackTrace_isNull = flyweight.GetBoolean();
			if (!stackTrace_isNull)
			{
				stacktrace = flyweight.GetStringUtf8();
			}
		}

		public static ExceptionResultCodec Decode(ClientMessage flyweight)
		{
			return new ExceptionResultCodec(flyweight);
		}

		public static ClientMessage Encode(string className, string causeClassName, string message, string stacktrace)
		{
			int requiredDataSize = CalculateDataSize(className, causeClassName, message, stacktrace);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(Type);
			clientMessage.Set(className);
			bool causeClassName_isNull = causeClassName == null;
			clientMessage.Set(causeClassName_isNull);
			if (!causeClassName_isNull)
			{
				clientMessage.Set(causeClassName);
			}
			bool message_isNull = message == null;
			clientMessage.Set(message_isNull);
			if (!message_isNull)
			{
				clientMessage.Set(message);
			}
			bool stackTrace_isNull = stacktrace == null;
			clientMessage.Set(stackTrace_isNull);
			if (!stackTrace_isNull)
			{
				clientMessage.Set(stacktrace);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static int CalculateDataSize(string className, string causeClassName, string message, string stacktrace)
		{
			int dataSize = ClientMessage.HeaderSize + ParameterUtil.CalculateStringDataSize(className);
			if (causeClassName == null)
			{
				dataSize += Bits.BooleanSizeInBytes;
			}
			else
			{
				dataSize += ParameterUtil.CalculateStringDataSize(causeClassName);
			}
			if (message == null)
			{
				dataSize += Bits.BooleanSizeInBytes;
			}
			else
			{
				dataSize += ParameterUtil.CalculateStringDataSize(message);
			}
			if (stacktrace == null)
			{
				dataSize += Bits.BooleanSizeInBytes;
			}
			else
			{
				dataSize += ParameterUtil.CalculateStringDataSize(stacktrace);
			}
			return dataSize;
		}
	}
}
