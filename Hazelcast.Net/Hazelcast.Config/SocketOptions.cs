using Hazelcast.Client.Connection;
using Hazelcast.Config;


namespace Hazelcast.Config
{
	
	public class SocketOptions
	{
		private bool tcpNoDelay = false;

		private bool keepAlive = true;

		private bool reuseAddress = true;

		private int lingerSeconds = 3;

		private int timeout = -1;

		private int bufferSize = 32;

		private ISocketFactory socketFactory;

		// socket options
		// in kb
		public virtual bool IsTcpNoDelay()
		{
			return tcpNoDelay;
		}

		public virtual SocketOptions SetTcpNoDelay(bool tcpNoDelay)
		{
			this.tcpNoDelay = tcpNoDelay;
			return this;
		}

		public virtual bool IsKeepAlive()
		{
			return keepAlive;
		}

		public virtual SocketOptions SetKeepAlive(bool keepAlive)
		{
			this.keepAlive = keepAlive;
			return this;
		}

		public virtual bool IsReuseAddress()
		{
			return reuseAddress;
		}

		public virtual SocketOptions SetReuseAddress(bool reuseAddress)
		{
			this.reuseAddress = reuseAddress;
			return this;
		}

		public virtual int GetLingerSeconds()
		{
			return lingerSeconds;
		}

		public virtual SocketOptions SetLingerSeconds(int lingerSeconds)
		{
			this.lingerSeconds = lingerSeconds;
			return this;
		}

		public virtual int GetTimeout()
		{
			return timeout;
		}

		public virtual SocketOptions SetTimeout(int timeout)
		{
			this.timeout = timeout;
			return this;
		}

		public virtual int GetBufferSize()
		{
			return bufferSize;
		}

		public virtual SocketOptions SetBufferSize(int bufferSize)
		{
			this.bufferSize = bufferSize;
			return this;
		}

		public virtual ISocketFactory GetSocketFactory()
		{
			return socketFactory;
		}

		public virtual SocketOptions SetSocketFactory(ISocketFactory socketFactory)
		{
			this.socketFactory = socketFactory;
			return this;
		}
	}
}
