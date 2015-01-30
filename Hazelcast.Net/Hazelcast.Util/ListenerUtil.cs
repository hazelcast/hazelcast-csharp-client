using System;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    /// <summary>
    ///     Lister add remove util
    /// </summary>
    internal class ListenerUtil
    {
        public static String Listen(ClientContext context, ClientRequest request, Object key,DistributedEventHandler handler)
        {
            try {
                Task<IData> task;
                if (key == null) {
                    task = context.GetInvocationService().InvokeOnRandomTarget(request, handler);
                }
                else
                {
                    task = context.GetInvocationService().InvokeOnKeyOwner(request, key, handler);
                }
                var registrationId = context.GetSerializationService().ToObject<string>(ThreadUtil.GetResult(task));
                context.GetRemotingService().RegisterListener(registrationId, request.CallId);
                return registrationId;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public static bool StopListening(ClientContext context, BaseClientRemoveListenerRequest request, String registrationId)
        {
            try
            {
                var unregistrationId = context.GetRemotingService().UnregisterListener(registrationId);

                if (unregistrationId)
                {
                    return false;
                }
                request.SetRegistrationId(registrationId);
                Task<IData> task = context.GetInvocationService().InvokeOnRandomTarget(request);
                var result = context.GetSerializationService().ToObject<bool>(ThreadUtil.GetResult(task));
                return result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }
    }
}
