// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using Hazelcast.Client.Protocol;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ClientListenerService : IClientListenerService
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientListenerService));
        private readonly HazelcastClient _client;

        private readonly ConcurrentDictionary<string, string> _registrationAliasMap =
            new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<string, long> _registrationMap = new ConcurrentDictionary<string, long>();

        public ClientListenerService(HazelcastClient hazelcastClient)
        {
            _client = hazelcastClient;
        }

        public string StartListening(IClientMessage request, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder, object key = null)
        {
            try
            {
                IFuture<IClientMessage> task;
                if (key == null)
                {
                    task = _client.GetInvocationService()
                        .InvokeListenerOnRandomTarget(request, handler, responseDecoder);
                }
                else
                {
                    task = _client.GetInvocationService()
                        .InvokeListenerOnKeyOwner(request, key, handler, responseDecoder);
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

        public bool StopListening(EncodeStopListenerRequest requestEncoder, DecodeStopListenerResponse responseDecoder,
            string registrationId)
        {
            try
            {
                var realRegistrationId = UnregisterListener(registrationId);

                if (realRegistrationId == null)
                {
                    Logger.Warning("Could not find the registration id alias for " + registrationId);
                    return false;
                }

                var request = requestEncoder(realRegistrationId);
                var task = _client.GetInvocationService().InvokeOnRandomTarget(request);
                var actualResult = responseDecoder(ThreadUtil.GetResult(task));
                if (Logger.IsFinestEnabled() && !actualResult)
                {
                    Logger.Finest("Remove listener response returned false from server for registration id " +
                                  registrationId);
                }
                return true;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public void ReregisterListener(string originalRegistrationId, string newRegistrationId, long correlationId)
        {
            // re-register a listener with an alias.
            _registrationAliasMap.AddOrUpdate(originalRegistrationId, newRegistrationId, (key, oldValue) =>
            {
                long ignored;
                _registrationMap.TryRemove(oldValue, out ignored);
                _registrationMap.TryAdd(newRegistrationId, correlationId);
                return newRegistrationId;
            });
        }

        private void RegisterListener(string registrationId, long callId)
        {
            _registrationAliasMap.TryAdd(registrationId, registrationId);
            _registrationMap.TryAdd(registrationId, callId);
        }

        private string UnregisterListener(string registrationId)
        {
            string uuid;
            if (_registrationAliasMap.TryRemove(registrationId, out uuid))
            {
                long correlationId;
                if (_registrationMap.TryRemove(uuid, out correlationId))
                {
                    _client.GetInvocationService().RemoveEventHandler(correlationId);
                }
            }
            return uuid;
        }
    }
}