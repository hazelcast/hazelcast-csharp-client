using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Hazelcast.Net.Ext;
using Hazelcast.Net.Ext;
using Hazelcast.Util;


namespace Hazelcast.Client.Spi
{
	public sealed class ListenerSupport<TE>:IListenerSupport
	{
		private readonly ClientContext context;

        private EventHandler<TE> handler;

		private readonly object registrationRequest;

		private Task<object> future;

		private volatile bool active = true;

		private volatile IResponseStream lastStream;

		private object partitionKey;

        internal readonly CountdownEvent latch = new CountdownEvent(1);

        internal CancellationTokenSource cancellation = new CancellationTokenSource();


		public ListenerSupport(ClientContext context, object registrationRequest,EventHandler<TE> handler)
		{
			this.context = context;
			this.registrationRequest = registrationRequest;
		    this.handler = handler;
		}

		public ListenerSupport(ClientContext context, object registrationRequest, EventHandler<TE> handler, object partitionKey) : this(context, registrationRequest, handler)
		{
			this.partitionKey = partitionKey;
		}

		public string Listen()
		{
		    Task listenerTask = Task.Factory.StartNew(delegate()
		    {
		        while (active && !Thread.CurrentThread.IsInterrupted())
		        {
		            try
		            {
                        
		                if (partitionKey == null)
		                {
		                    context.GetInvocationService().InvokeOnRandomTarget(registrationRequest, EventResponseHandler);
		                }
		                else
		                {
		                    context.GetInvocationService().InvokeOnKeyOwner(registrationRequest, partitionKey, EventResponseHandler);
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
					TE _event = (TE) stream.Read();
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
				else
				{
					active = false;
				}
			}
		}
	}

    internal class EventData : EventArgs
    {
        private object data;

        public object Data
        {
            get { return data; }
            set { data = value; }
        }

        public EventData(object data)
        {
            this.data = data;
        }
    }

    public interface IListenerSupport
    {
        string Listen();
        void Stop();
    }
}
