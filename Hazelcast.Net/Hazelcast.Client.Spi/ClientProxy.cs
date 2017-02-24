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
        private readonly string _objectName;
        private readonly string _serviceName;
        private volatile ClientContext _context;


        protected ClientProxy(string serviceName, string objectName)
        {
            _serviceName = serviceName;
            _objectName = objectName;
        }

        public string GetPartitionKey()
        {
            return StringPartitioningStrategy.GetPartitionKey(GetName());
        }

        public string GetName()
        {
            return _objectName;
        }

        public string GetServiceName()
        {
            return _serviceName;
        }

        public void Destroy()
        {
            OnDestroy();
            var request = ClientDestroyProxyCodec.EncodeRequest(_objectName, GetServiceName());
            Invoke(request);
            _context.RemoveProxy(this);
            _context = null;
        }

        public virtual Task<T> InvokeAsync<T>(IClientMessage request, object key,
            Func<IClientMessage, T> decodeResponse)
        {
            var future = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
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

        protected internal virtual void ThrowExceptionIfNull(object o, string message = null)
        {
            if (o == null)
            {
                throw new ArgumentNullException(message);
            }
        }

        protected internal virtual void ThrowExceptionIfTrue(bool expression, string message)
        {
            if (expression)
            {
                throw new ArgumentException(message);
            }
        }

        protected internal virtual IData ToData(object o)
        {
            return GetContext().GetSerializationService().ToData(o);
        }

        protected internal virtual ISet<KeyValuePair<TKey, TValue>> ToEntrySet<TKey, TValue>(
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

        protected internal virtual IList<T> ToList<T>(ICollection<IData> dataList)
        {
            var list = new List<T>(dataList.Count);
            foreach (var data in dataList)
            {
                list.Add(ToObject<T>(data));
            }
            return list;
        }

        protected internal virtual T ToObject<T>(IData data)
        {
            return GetContext().GetSerializationService().ToObject<T>(data);
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

        protected virtual ClientContext GetContext()
        {
            var ctx = _context;
            if (ctx == null)
            {
                throw new HazelcastInstanceNotActiveException();
            }
            return ctx;
        }

        protected virtual IClientMessage Invoke(IClientMessage request, object key)
        {
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        protected virtual T Invoke<T>(IClientMessage request, object key, Func<IClientMessage, T> decodeResponse)
        {
            var response = Invoke(request, key);
            return decodeResponse(response);
        }

        protected virtual IClientMessage Invoke(IClientMessage request)
        {
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnRandomTarget(request);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        protected virtual T Invoke<T>(IClientMessage request, Func<IClientMessage, T> decodeResponse)
        {
            var response = Invoke(request);
            return decodeResponse(response);
        }

        protected virtual string Listen(ClientMessage registrationRequest, DecodeStartListenerResponse responseDecoder,
            object partitionKey, DistributedEventHandler handler)
        {
            return _context.GetListenerService()
                .StartListening(registrationRequest, handler, responseDecoder, partitionKey);
        }

        protected virtual string Listen(ClientMessage registrationRequest, DecodeStartListenerResponse responseDecoder,
            DistributedEventHandler handler)
        {
            return _context.GetListenerService().StartListening(registrationRequest, handler, responseDecoder);
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual bool StopListening(EncodeStopListenerRequest responseEncoder,
            DecodeStopListenerResponse responseDecoder, string registrationId)
        {
            return _context.GetListenerService().StopListening(responseEncoder, responseDecoder, registrationId);
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

        protected IDictionary<TKey, object> DeserializeEntries<TKey>(IList<KeyValuePair<IData, IData>> entries) {
            if (entries.Count == 0)
            {
                return new Dictionary<TKey, object>();
            }
            var result = new Dictionary<TKey, object>();
            foreach(var entry in entries) {
                var key = (TKey)ToObject<object>(entry.Key);
                result.Add(key, ToObject<object>(entry.Value));
            }
            return result;
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

        internal virtual void PostInit()
        {
        }

        internal virtual void SetContext(ClientContext context)
        {
            _context = context;
        }
    }
}