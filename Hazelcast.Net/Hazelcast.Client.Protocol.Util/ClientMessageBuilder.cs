using System;
using System.Collections.Generic;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Util
{
    /// <summary>
    ///     Builds
    ///     <see cref="Hazelcast.Client.Protocol.ClientMessage" />
    ///     s from byte chunks. Fragmented messages are merged into single messages before processed.
    /// </summary>
    internal class ClientMessageBuilder
    {
        /// <summary>Implementers will be responsible to delegate the constructed message</summary>
        public delegate void HandleMessageDelegate(ClientMessage message);

        private readonly HandleMessageDelegate _delegate;
        private readonly Dictionary<int, BufferBuilder> builderBySessionIdMap = new Dictionary<int, BufferBuilder>();
        private ClientMessage message = ClientMessage.Create();

        public ClientMessageBuilder(HandleMessageDelegate _delegate)
        {
            this._delegate = _delegate;
        }

        public virtual void OnData(ByteBuffer buffer)
        {
            while (buffer.HasRemaining())
            {
                var complete = message.ReadFrom(buffer);
                if (!complete)
                {
                    return;
                }
                //MESSAGE IS COMPLETE HERE
                if (message.IsFlagSet(ClientMessage.BeginAndEndFlags))
                {
                    //HANDLE-MESSAGE
                    HandleMessage(message);
                    message = ClientMessage.Create();
                    continue;
                }
                if (message.IsFlagSet(ClientMessage.BeginFlag))
                {
                    // first fragment
                    var builder = new BufferBuilder();
                    builderBySessionIdMap.Add(message.GetCorrelationId(), builder);
                    builder.Append(message.Buffer(), 0, message.GetFrameLength());
                }
                else
                {
                    var builder = builderBySessionIdMap[message.GetCorrelationId()];
                    if (builder.Position() == 0)
                    {
                        throw new InvalidOperationException();
                    }
                    builder.Append(message.Buffer(), message.GetDataOffset(),
                        message.GetFrameLength() - message.GetDataOffset());
                    if (message.IsFlagSet(ClientMessage.EndFlag))
                    {
                        var msgLength = builder.Position();
                        var cm = ClientMessage.CreateForDecode(builder.Buffer(), 0);
                        cm.SetFrameLength(msgLength);
                        //HANDLE-MESSAGE
                        HandleMessage(cm);
                        builderBySessionIdMap.Remove(message.GetCorrelationId());
                    }
                }
                message = ClientMessage.Create();
            }
        }

        private void HandleMessage(ClientMessage message)
        {
            message.Index(message.GetDataOffset());
            _delegate(message);
        }
    }
}