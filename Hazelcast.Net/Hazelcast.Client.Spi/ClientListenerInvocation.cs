using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ClientListenerInvocation : ClientInvocation
    {
        private readonly DecodeStartListenerResponse _responseDecoder;
        private readonly DistributedEventHandler _handler;

        public ClientListenerInvocation(IClientMessage message, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder, int partitionId = -1, string memberUuid = null)
            : base(message, partitionId, memberUuid)
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