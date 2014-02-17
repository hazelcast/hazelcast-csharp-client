using System;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;

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
                Task<string> task = null;
                if (key == null) {
                    task = context.GetInvocationService().InvokeOnRandomTarget<string>(request, handler);
                }
                else
                {
                    task = context.GetInvocationService().InvokeOnKeyOwner<string>(request, key, handler);
                }
                var registrationId = context.GetSerializationService().ToObject<string>(task.Result);
                context.GetRemotingService().RegisterListener(registrationId, request.CallId);
                return registrationId;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public static bool StopListening(ClientContext context, ClientRequest request, String registrationId)
        {
            try
            {
                var unregistrationId = context.GetRemotingService().UnregisterListener(registrationId);

                if (unregistrationId == null)
                {
                    return false;
                }

                if (request is IRemoveRequest)
                {
                    ((IRemoveRequest)request).RegistrationId = registrationId;
                }

                Task<bool> task = context.GetInvocationService().InvokeOnRandomTarget<bool>(request);
                var result = context.GetSerializationService().ToObject<bool>(task.Result);

                return result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }
    }
}
