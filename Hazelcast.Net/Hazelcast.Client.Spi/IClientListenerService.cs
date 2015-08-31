using System;
using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public interface IClientListenerService
    {
        string StartListening(IClientMessage request, DecodeStartListenerResponse decodeResponse, DistributedEventHandler handler, object key = null);
        bool StopListening(IClientMessage request, String registrationId, DecodeStopListenerResponse decodeResponse);
    }
}
