using System;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    /// <summary>
    ///     Lister add remove util
    /// </summary>
    internal class ListenerUtil
    {
        public static String Listen(ClientContext context, IClientMessage request, DecodeStartListenerResponse decodeListenerResponse, Object key, DistributedEventHandler handler)
        {
            try {
                Task<IClientMessage> task;
                if (key == null) {
                    task = context.GetInvocationService().InvokeOnRandomTarget(request, handler);
                }
                else
                {
                    task = context.GetInvocationService().InvokeOnKeyOwner(request, key, handler);
                }
                var clientMessage = ThreadUtil.GetResult(task);
                var registrationId = decodeListenerResponse(clientMessage);
                context.GetRemotingService().RegisterListener(registrationId, request.GetCorrelationId());
                return registrationId;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public static bool StopListening(ClientContext context, ClientMessage request, DecodeStopListenerResponse decodeListenerResponse, String registrationId)
        {
            try
            {
                var unregistrationId = context.GetRemotingService().UnregisterListener(registrationId);

                if (unregistrationId)
                {
                    return false;
                }
                var task = context.GetInvocationService().InvokeOnRandomTarget(request);
                var result = decodeListenerResponse(ThreadUtil.GetResult(task));
                return result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }
    }
}
