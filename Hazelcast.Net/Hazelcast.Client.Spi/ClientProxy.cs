// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Partition.Strategy;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal abstract class ClientProxy : IDistributedObject
    {
        private readonly HazelcastClient _client;


        protected ClientProxy(string serviceName, string objectName, HazelcastClient client)
        {
            _client = client;
            ServiceName = serviceName;
            Name = objectName;
        }

        public string GetPartitionKey()
        {
            return StringPartitioningStrategy.GetPartitionKey(Name);
        }

        public string Name { get; }

        public string ServiceName { get; }

        protected virtual Task<T> InvokeAsync<T>(ClientMessage request, object key, Func<ClientMessage, T> decodeResponse)
        {
            var future = Client.InvocationService.InvokeOnKeyOwner(request, key);
            var continueTask = future.ToTask().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    throw t.Exception.Flatten().InnerExceptions.First();
                }
                var clientMessage = ThreadUtil.GetResult(t);
                return decodeResponse(clientMessage);
            });
            return continueTask;
        }

        protected virtual IData ToData(object Object)
        {
            return Client.SerializationService.ToData(Object);
        }

        protected virtual T ToObject<T>(object Object)
        {
            return Client.SerializationService.ToObject<T>(Object);
        }

        protected HazelcastClient Client
        {
            get
            {
                var ctx = _client;
                if (ctx == null)
                {
                    throw new HazelcastClientNotActiveException();
                }
                return ctx;
            }
        }

        // internal void SetContext(HazelcastClient client)
        // {
        //     _client = client;
        // }

        protected virtual ISet<KeyValuePair<TKey, TValue>> ToEntrySet<TKey, TValue>(
            ICollection<KeyValuePair<IData, IData>> entryCollection)
        {
            ISet<KeyValuePair<TKey, TValue>> entrySet = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (var entry in entryCollection)
            {
                var key = ToObject<TKey>(entry.Key);
                var val = ToObject<TValue>(entry.Value);
                entrySet.Add(new KeyValuePair<TKey, TValue>(key, val));
            }

            return entrySet;
        }

        protected virtual IList<T> ToList<T>(ICollection<IData> dataList)
        {
            var list = new List<T>(dataList.Count);
            foreach (var data in dataList)
            {
                list.Add(ToObject<T>(data));
            }

            return list;
        }

        protected internal virtual ISet<T> ToSet<T>(ICollection<IData> dataList)
        {
            var set = new HashSet<T>();
            foreach (var data in dataList)
            {
                set.Add(ToObject<T>(data));
            }

            return set;
        }

        protected IList<IData> ToDataList<T>(ICollection<T> c)
        {
            ValidationUtil.ThrowExceptionIfNull(c, "Collection cannot be null.");
            var values = new List<IData>(c.Count);
            foreach (var o in c)
            {
                ValidationUtil.ThrowExceptionIfNull(o, "Collection cannot contain null items.");
                values.Add(ToData(o));
            }

            return values;
        }

        protected IDictionary<TKey, object> DeserializeEntries<TKey>(IList<KeyValuePair<IData, IData>> entries)
        {
            if (entries.Count == 0)
            {
                return new Dictionary<TKey, object>();
            }

            var result = new Dictionary<TKey, object>();
            foreach (var entry in entries)
            {
                var key = (TKey) ToObject<object>(entry.Key);
                result.Add(key, ToObject<object>(entry.Value));
            }

            return result;
        }

        protected ISet<IData> ToDataSet<T>(ICollection<T> c)
        {
            ValidationUtil.ThrowExceptionIfNull(c);
            var valueSet = new HashSet<IData>();
            foreach (var o in c)
            {
                ValidationUtil.ThrowExceptionIfNull(o);
                valueSet.Add(ToData(o));
            }

            return valueSet;
        }

        protected ClientMessage InvokeOnTarget(ClientMessage request, Guid target)
        {
            try
            {
                var task = Client.InvocationService.InvokeOnTarget(request, target);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        protected virtual ClientMessage Invoke(ClientMessage request, object key)
        {
            try
            {
                var task = Client.InvocationService.InvokeOnKeyOwner(request, key);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        protected T Invoke<T>(ClientMessage request, object key, Func<ClientMessage, T> decodeResponse)
        {
            var response = Invoke(request, key);
            return decodeResponse(response);
        }

        protected T InvokeOnPartition<T>(ClientMessage request, int partitionId, Func<ClientMessage, T> decodeResponse)
        {
            var response = InvokeOnPartition(request, partitionId);
            return decodeResponse(response);
        }

        protected ClientMessage InvokeOnPartition(ClientMessage request, int partitionId)
        {
            try
            {
                var task = Client.InvocationService.InvokeOnPartitionOwner(request, partitionId);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        protected virtual ClientMessage Invoke(ClientMessage request)
        {
            try
            {
                var task = Client.InvocationService.InvokeOnRandomTarget(request);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        protected virtual T Invoke<T>(ClientMessage request, Func<ClientMessage, T> decodeResponse)
        {
            var response = Invoke(request);
            return decodeResponse(response);
        }

        protected virtual Guid RegisterListener(ClientMessage registrationMessage, DecodeRegisterResponse responseDecoder,
            EncodeDeregisterRequest encodeDeregisterRequest, DistributedEventHandler eventHandler)
        {
            return Client.ListenerService
                .RegisterListener(registrationMessage, responseDecoder, encodeDeregisterRequest, eventHandler);
        }

        protected virtual bool DeregisterListener(Guid userRegistrationId)
        {
            return Client.ListenerService.DeregisterListener(userRegistrationId);
        }

        protected virtual bool IsSmart()
        {
            return Client.ClientConfig.GetNetworkConfig().IsSmartRouting();
        }

        public void Destroy() 
        {
            _client.ProxyManager.DestroyProxy(this);
        }
        public void DestroyLocally()
        {
            if (PreDestroy()) {
                try {
                    OnDestroy();
                } finally {
                    PostDestroy();
                }
            }
        }

        public void DestroyRemotely()
        {
            var clientMessage = ClientDestroyProxyCodec.EncodeRequest(Name, ServiceName);
            Invoke(clientMessage);
        }

        protected internal virtual void OnInitialize()
        {
        }

        protected internal virtual void OnDestroy()
        {
        }

        protected internal virtual void OnShutdown()
        {
        }

        protected internal virtual bool PreDestroy() {
            return true;
        }

        protected internal virtual void PostDestroy()
        {
        }
    }
}