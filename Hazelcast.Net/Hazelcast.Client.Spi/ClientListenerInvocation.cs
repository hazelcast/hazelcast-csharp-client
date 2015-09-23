using Hazelcast.Client.Protocol;
using Hazelcast.IO;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ClientListenerInvocation : ClientInvocation
    {
        private readonly DecodeStartListenerResponse _responseDecoder;
        private readonly DistributedEventHandler _handler;

        public ClientListenerInvocation(IClientMessage message, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
            : base(message)
        {
            _responseDecoder = responseDecoder;
            _handler = handler;
        }

        public ClientListenerInvocation(IClientMessage message, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder, int partitionId)
            : base(message, partitionId)
        {
            _responseDecoder = responseDecoder;
            _handler = handler;
        }

        public ClientListenerInvocation(IClientMessage message, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder, string memberUuid)
            : base(message, memberUuid)
        {
            _responseDecoder = responseDecoder;
            _handler = handler;
        }

        public ClientListenerInvocation(IClientMessage message, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder, Address address)
            : base(message, address)
        {
            _responseDecoder = responseDecoder;
            _handler = handler;
        }

        public DistributedEventHandler Handler
        {
            get { return _handler; }
        }

        public DecodeStartListenerResponse ResponseDecoder
        {
            get { return _responseDecoder; }
        }
    }
}