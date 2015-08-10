using System;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Request.Base;
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
            try
            {
                context.GetInvocationService().InvokeOnRandomTarget(request);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            context.RemoveProxy(this);
            context = null;
        }

        protected virtual string Listen(ClientMessage registrationRequest, DecodeStartListenerResponse decodeListenerResponse, object partitionKey, DistributedEventHandler handler)
        {
            return ListenerUtil.Listen(context, registrationRequest, decodeListenerResponse, partitionKey, handler);
        }

        protected virtual string Listen(ClientMessage registrationRequest, DecodeStartListenerResponse decodeListenerResponse, DistributedEventHandler handler)
        {
            return ListenerUtil.Listen(context, registrationRequest, decodeListenerResponse, null, handler);
        }

        protected virtual bool StopListening(ClientMessage registrationRequest, DecodeStopListenerResponse decodeListenerResponse, string registrationId)
        {
            return ListenerUtil.StopListening(context, registrationRequest, decodeListenerResponse, registrationId);
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
        protected internal virtual void ThrowExceptionIfNull(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
        }

        internal virtual void PostInit(){}

    }

}
