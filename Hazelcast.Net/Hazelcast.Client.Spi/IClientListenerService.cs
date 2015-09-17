using System;
using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public interface IClientListenerService
    {
        string StartListening(IClientMessage request, DistributedEventHandler handler, DecodeStartListenerResponse responseDecoder, object key = null);
        bool StopListening(EncodeStopListenerRequest requestEncoder, DecodeStopListenerResponse responseDecoder, string registrationId);
        void ReregisterListener(string originalRegistrationId, string newRegistrationId, int correlationId);
    }
}
