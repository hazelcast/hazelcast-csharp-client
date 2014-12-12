using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
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
            var request = new ClientDestroyRequest(objectName, GetServiceName());
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

        protected virtual string Listen(ClientRequest registrationRequest, Object partitionKey, DistributedEventHandler handler)
        {
            return ListenerUtil.Listen(context, registrationRequest, partitionKey, handler);
        }


        protected virtual string Listen(ClientRequest registrationRequest, DistributedEventHandler handler)
        {
            return ListenerUtil.Listen(context, registrationRequest, null, handler);
        }

        protected virtual bool StopListening(BaseClientRemoveListenerRequest request, string registrationId)
        {
            return ListenerUtil.StopListening(context, request, registrationId);
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

        protected abstract void OnDestroy();

        protected virtual T Invoke<T>(ClientRequest request, object key)
        {
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
                task.Wait(TimeSpan.FromSeconds(60));
                var result = task.Result;
                return context.GetSerializationService().ToObject<T>(result);
            }
            catch (Exception e) {
                throw ExceptionUtil.Rethrow(e);
            }  
        }
        protected virtual T Invoke<T>(ClientRequest request)
        {
            try {
                var task = GetContext().GetInvocationService().InvokeOnRandomTarget(request);
                return context.GetSerializationService().ToObject<T>(task.Result);
            } catch (Exception e) {
                throw ExceptionUtil.Rethrow(e);
            }  
        }

        protected internal virtual IData ToData(object o)
        {
            return GetContext().GetSerializationService().ToData(o);
        }

        protected internal virtual T ToObject<T>(IData data)
        {
            return GetContext().GetSerializationService().ToObject<T>(data);
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
