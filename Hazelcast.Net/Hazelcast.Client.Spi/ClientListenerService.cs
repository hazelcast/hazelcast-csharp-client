using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ClientListenerService : IClientListenerService
    {

        private readonly ConcurrentDictionary<string, string> _registrationAliasMap =
            new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<string, int> _registrationMap = new ConcurrentDictionary<string, int>();

        private HazelcastClient _client;

        public ClientListenerService(HazelcastClient hazelcastClient)
        {
            _client = hazelcastClient;
        }

        public string StartListening(IClientMessage request, DecodeStartListenerResponse decodeListenerResponse, DistributedEventHandler handler, Object key = null)
        {
            try
            {
                IFuture<IClientMessage> task;
                if (key == null)
                {
                    task = _client.GetInvocationService().InvokeOnRandomTarget(request, handler);
                }
                else
                {
                    task = _client.GetInvocationService().InvokeOnKeyOwner(request, key, handler);
                }
                var clientMessage = ThreadUtil.GetResult(task);
                var registrationId = decodeListenerResponse(clientMessage);
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
                var unregistered = UnregisterListener(registrationId);

                if (unregistered)
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
      
        private void RegisterListener(string registrationId, int callId)
        {
            _registrationAliasMap.TryAdd(registrationId, registrationId);
            _registrationMap.TryAdd(registrationId, callId);
        }

        private bool UnregisterListener(string registrationId)
        {
            string uuid;
            if (_registrationAliasMap.TryRemove(registrationId, out uuid))
            {
                int callId;
                if (_registrationMap.TryRemove(registrationId, out callId))
                {
                    return _client.GetInvocationService().RemoveEventHandler(callId);
                }
            }
            return false;
        }

        private void ReRegisterListener(string uuidregistrationId, string alias, int callId)
        {
            string oldAlias;
            if (_registrationAliasMap.TryRemove(uuidregistrationId, out oldAlias))
            {
                int removed;
                _registrationMap.TryRemove(oldAlias, out removed);
                _registrationMap.TryAdd(alias, callId);
            }
            _registrationAliasMap.TryAdd(uuidregistrationId, alias);
        }
    }
}