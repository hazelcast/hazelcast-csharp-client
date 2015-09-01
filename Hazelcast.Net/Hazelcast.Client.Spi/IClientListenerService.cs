using System;
using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public interface IClientListenerService
    {
        string StartListening(IClientMessage request, DistributedEventHandler handler, DecodeStartListenerResponse responseDecoder, object key = null);
        bool StopListening(IClientMessage request, String registrationId, DecodeStopListenerResponse decodeResponse);
        void ReregisterListener(string uuid, string alias, int correlationId);
    }
}
