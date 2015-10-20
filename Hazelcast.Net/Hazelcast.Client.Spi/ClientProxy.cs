/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Partition.Strategy;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal abstract class ClientProxy : IDistributedObject
    {
        private readonly string objectName;
        private readonly string serviceName;
        private volatile ClientContext context;

   
        protected ClientProxy(String serviceName, String objectName) 
        {
            this.serviceName = serviceName;
            this.objectName = objectName;
        }

        public string GetPartitionKey()
        {
            return StringPartitioningStrategy.GetPartitionKey(GetName());
        }

        public string GetName()
        {
           return objectName;
        }

        public string GetServiceName()
        {
            return serviceName;
        }

        public void Destroy()
        {
            OnDestroy();
            var request = ClientDestroyProxyCodec.EncodeRequest(objectName, GetServiceName());
            Invoke(request);
            context.RemoveProxy(this);
            context = null;
        }

        protected virtual string Listen(ClientMessage registrationRequest, DecodeStartListenerResponse responseDecoder, object partitionKey, DistributedEventHandler handler)
        {
            return context.GetListenerService().StartListening(registrationRequest, handler, responseDecoder, partitionKey);
        }

        protected virtual string Listen(ClientMessage registrationRequest, DecodeStartListenerResponse responseDecoder, DistributedEventHandler handler)
        {
            return context.GetListenerService().StartListening(registrationRequest, handler, responseDecoder);
        }

        protected virtual bool StopListening(EncodeStopListenerRequest responseEncoder, DecodeStopListenerResponse responseDecoder, string registrationId)
        {
            return context.GetListenerService().StopListening(responseEncoder, responseDecoder, registrationId);
        }

        protected virtual ClientContext GetContext()
        {
            ClientContext ctx = context;
            if (ctx == null)
            {
                throw new HazelcastInstanceNotActiveException();
            }
            return ctx;
        }

        internal virtual void SetContext(ClientContext context)
        {
            this.context = context;
        }

        protected virtual void OnDestroy()
        {
            
        }

        protected virtual IClientMessage Invoke(IClientMessage request, object key)
        {
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e) {
                throw ExceptionUtil.Rethrow(e);
            }  
        }

        protected virtual T Invoke<T>(IClientMessage request, object key, Func<IClientMessage, T> decodeResponse)
        {
            var response = Invoke(request, key);
            return decodeResponse(response);
        }

        public virtual Task<T> InvokeAsync<T>(IClientMessage request, object key,
            Func<IClientMessage, T> decodeResponse)
        {
            var future = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
            var continueTask = future.ToTask().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = t.Exception;
                    throw exception.InnerExceptions.First();
                }
                var clientMessage = ThreadUtil.GetResult(t);
                return decodeResponse(clientMessage);
            });
            return continueTask;
        }

        protected virtual IClientMessage Invoke(IClientMessage request)
        {
            try {
                var task = GetContext().GetInvocationService().InvokeOnRandomTarget(request);
                return ThreadUtil.GetResult(task);
            } catch (Exception e) {
                throw ExceptionUtil.Rethrow(e);
            }  
        }

        protected virtual T Invoke<T>(IClientMessage request, Func<IClientMessage, T> decodeResponse)
        {
            var response = Invoke(request);
            return decodeResponse(response);
        }

        protected internal virtual IData ToData(object o)
        {
            return GetContext().GetSerializationService().ToData(o);
        }

        protected ISet<IData> ToDataSet<T>(ICollection<T> c)
        {
            ThrowExceptionIfNull(c);
            var valueSet = new HashSet<IData>();
            foreach (var o in c)
            {
                ThrowExceptionIfNull(o);
                valueSet.Add(ToData(o));
            }
            return valueSet;
        }

        protected IList<IData> ToDataList<T>(ICollection<T> c)
        {
            ThrowExceptionIfNull(c, "Collection cannot be null.");
            var values = new List<IData>(c.Count);
            foreach (var o in c)
            {
                ThrowExceptionIfNull(o, "Collection cannot contain null items.");
                values.Add(ToData(o));
            }
            return values;
        }
        protected internal virtual T ToObject<T>(IData data)
        {
            return GetContext().GetSerializationService().ToObject<T>(data);
        }

        protected internal virtual IList<T> ToList<T>(ICollection<IData> dataList)
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

        protected internal virtual ISet<KeyValuePair<K, V>> ToEntrySet<K, V>(
            ICollection<KeyValuePair<IData, IData>> entryCollection)
        {
            ISet<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (var entry in entryCollection)
            {
                var key = ToObject<K>(entry.Key);
                var val = ToObject<V>(entry.Value);
                entrySet.Add(new KeyValuePair<K, V>(key, val));
            }
            return entrySet;
        }

        protected internal virtual void ThrowExceptionIfNull(object o, String message = null)
        {
            if (o == null)
            {
                throw new ArgumentNullException(message);
            }
        }

        protected internal virtual void ThrowExceptionIfTrue(bool expression, String message)
        {
            if (expression)
            {
                throw new ArgumentException(message);
            }
        }
        internal virtual void PostInit(){}

    }

}
