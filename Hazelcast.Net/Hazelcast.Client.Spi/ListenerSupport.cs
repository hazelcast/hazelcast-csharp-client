using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public sealed class ListenerSupport<TE> : IListenerSupport where TE : EventArgs
    {
        private readonly ClientContext context;

        private readonly EventHandler<TE> handler;
        internal readonly CountdownEvent latch = new CountdownEvent(1);
        private readonly object partitionKey;

        private readonly object registrationRequest;

        private volatile bool active = true;

        internal CancellationTokenSource cancellation = new CancellationTokenSource();
        private Task<object> future;
        private volatile IResponseStream lastStream;


        public ListenerSupport(ClientContext context, object registrationRequest, EventHandler<TE> handler)
        {
            this.context = context;
            this.registrationRequest = registrationRequest;
            this.handler = handler;
        }

        public ListenerSupport(ClientContext context, object registrationRequest, EventHandler<TE> handler,
            object partitionKey) : this(context, registrationRequest, handler)
        {
            this.partitionKey = partitionKey;
        }

        public string Listen()
        {
            Task listenerTask = Task.Factory.StartNew(delegate
            {
                while (active && !Thread.CurrentThread.IsInterrupted())
                {
                    try
                    {
                        if (partitionKey == null)
                        {
                            context.GetInvocationService()
                                .InvokeOnRandomTarget(registrationRequest, EventResponseHandler);
                        }
                        else
                        {
                            context.GetInvocationService()
                                .InvokeOnKeyOwner(registrationRequest, partitionKey, EventResponseHandler);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }, cancellation.Token);
            try
            {
                if (!latch.Wait(TimeSpan.FromMinutes(1)))
                {
                    throw new HazelcastException("Could not register listener!!!");
                }
            }
            catch (Exception)
            {
            }
            return Guid.NewGuid().ToString();
        }

        public void Stop()
        {
            active = false;
            if (cancellation != null)
            {
                cancellation.Cancel();
            }
            IResponseStream s = lastStream;
            if (s != null)
            {
                try
                {
                    s.End();
                }
                catch (IOException)
                {
                }
            }
        }

        private void EventResponseHandler(IResponseStream stream)
        {
            try
            {
                stream.Read();
                // initial ok response
                lastStream = stream;
                latch.Signal();
                while (active && ! Thread.CurrentThread.IsInterrupted())
                {
                    var _event = (TE) stream.Read();
                    handler(this, _event);
                }
            }
            catch (Exception e)
            {
                try
                {
                    stream.End();
                }
                catch (IOException)
                {
                }
                if (ErrorHandler.IsRetryable(e))
                {
                    throw;
                }
                active = false;
            }
        }
    }

    internal class EventData : EventArgs
    {
        public EventData(object data)
        {
            this.Data = data;
        }

        public object Data { get; set; }
    }

    public interface IListenerSupport
    {
        string Listen();
        void Stop();
    }
}