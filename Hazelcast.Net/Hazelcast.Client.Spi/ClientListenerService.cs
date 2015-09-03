using System;
using System.Collections.Concurrent;
using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ClientListenerService : IClientListenerService
    {

        private readonly ConcurrentDictionary<string, string> _registrationAliasMap =
            new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<string, int> _registrationMap = new ConcurrentDictionary<string, int>();

        private readonly HazelcastClient _client;

        public ClientListenerService(HazelcastClient hazelcastClient)
        {
            _client = hazelcastClient;
        }

        public string StartListening(IClientMessage request, DistributedEventHandler handler, DecodeStartListenerResponse responseDecoder, object key = null)
        {
            try
            {
                IFuture<IClientMessage> task;
                if (key == null)
                {
                    task = _client.GetInvocationService().InvokeListenerOnRandomTarget(request, handler, responseDecoder);
                }
                else
                {
                    task = _client.GetInvocationService().InvokeListenerOnKeyOwner(request, key, handler, responseDecoder);
                }
                var clientMessage = ThreadUtil.GetResult(task);
                var registrationId = responseDecoder(clientMessage);
                RegisterListener(registrationId, request.GetCorrelationId());
                return registrationId;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public bool StopListening(IClientMessage request, string registrationId, DecodeStopListenerResponse decodeListenerResponse)
        {
            try
            {
                var realRegistrationId = UnregisterListener(registrationId);

                if (realRegistrationId == null)
                {
                    return false;
                }

                var task = _client.GetInvocationService().InvokeOnRandomTarget(request);
                var result = decodeListenerResponse(ThreadUtil.GetResult(task));
                return result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public void ReregisterListener(string uuid, string alias, int correlationId)
        {
            // re-register a listener with an alias.
            _registrationAliasMap.AddOrUpdate(uuid, alias, (key, oldValue) =>
            {
                int ignored;
                _registrationMap.TryRemove(oldValue, out ignored);
                _registrationMap.TryAdd(alias, correlationId);
                return alias;
            });
        }

        private void RegisterListener(string registrationId, int callId)
        {
            _registrationAliasMap.TryAdd(registrationId, registrationId);
            _registrationMap.TryAdd(registrationId, callId);
        }

        private string UnregisterListener(string registrationId)
        {
            string uuid;
            if (_registrationAliasMap.TryRemove(registrationId, out uuid))
            {
                int correlationId;
                if (_registrationMap.TryRemove(registrationId, out correlationId))
                {
                    _client.GetInvocationService().RemoveEventHandler(correlationId);
                }
            }
            return uuid;
        }
    }
}