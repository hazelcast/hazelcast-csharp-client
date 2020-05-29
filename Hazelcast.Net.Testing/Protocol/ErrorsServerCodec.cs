using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Protocol.Data;

namespace Hazelcast.Testing.Protocol
{
    internal static class ErrorsServerCodec
    {
        // Other codecs message types can be in range 0x000100 - 0xFFFFFF
        // So, it is safe to supply a custom message type for exceptions in
        // the range 0x000000 - 0x0000FF
        public const int ExceptionMessageType = 0;
        private const int InitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfInt;

        // or should this be available for all codecs, EncodeResponse, in the testing project?
        public static ClientMessage EncodeResponse(IEnumerable<ErrorHolder> errorHolders)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[InitialFrameSize]);
            clientMessage.Append(initialFrame);
            clientMessage.MessageType = ExceptionMessageType;
            ListMultiFrameCodec.Encode(clientMessage, errorHolders, ErrorHolderCodec.Encode);
            return clientMessage;
        }
    }
}
