using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;


namespace Hazelcast.Client.Connection
{
	/// <summary>Holds the clientSocket to one of the members of Hazelcast ICluster.</summary>
	/// <remarks>Holds the clientSocket to one of the members of Hazelcast ICluster.</remarks>
	
	internal sealed class Connection : IConnection
	{
		private static int ConnId = 1;

		private const int BufferSize = 16 << 10;

		// 32k
		private static int NewConnId()
		{
			lock (typeof(Connection))
			{
				return ConnId++;
			}
		}

		private readonly ObjectDataOutputStream _out;

		private readonly ObjectDataInputStream _in;

		private readonly int id = NewConnId();

		private volatile Address _endpoint;

		private long lastRead = Clock.CurrentTimeMillis();

        private volatile Socket clientSocket;

	    /// <exception cref="System.IO.IOException"></exception>
		public Connection(Address address, SocketOptions options, ISerializationService serializationService)
		{
			IPEndPoint isa = address.GetInetSocketAddress();
			ISocketFactory socketFactory = options.GetSocketFactory();
			if (socketFactory == null)
			{
				socketFactory = new DefaultSocketFactory();
			}
			clientSocket = socketFactory.CreateSocket();
			try
			{
                clientSocket = new Socket(isa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                var lingerOption = new LingerOption(true, 5);
                if (options.GetLingerSeconds() > 0)
                {
                    lingerOption.LingerTime =  options.GetLingerSeconds();
                
                } 
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);


				clientSocket.NoDelay = options.IsTcpNoDelay();

                //TODO BURASI NOLCAK
				//clientSocket.ExclusiveAddressUse SetReuseAddress(options.IsReuseAddress());

				if (options.GetTimeout() > 0)
				{
					clientSocket.ReceiveTimeout = options.GetTimeout();
				}
				int bufferSize = options.GetBufferSize() * 1024;
				if (bufferSize < 0)
				{
					bufferSize = BufferSize;
				}

				clientSocket.SendBufferSize= bufferSize;
			    clientSocket.ReceiveBufferSize = bufferSize;
				
                clientSocket.Connect(address.GetHost(), address.GetPort());

			    var netStream = new NetworkStream(clientSocket, true);
                var bufStream = new BufferedStream(netStream, bufferSize);

			   // new BufferedOutputStream(clientSocket.GetOutputStream(), bufferSize);
			    var writer = new BinaryWriter(bufStream);
			    var reader = new BinaryReader(bufStream);

                _out = serializationService.CreateObjectDataOutputStream(writer);
				_in = serializationService.CreateObjectDataInputStream(reader);
			}
			catch (Exception e)
			{
				clientSocket.Close();
				throw new IOException("Socket error:",e);
			}
		}

		internal Socket GetSocket()
		{
            return clientSocket;
		}

		public Address GetRemoteEndpoint()
		{
			return _endpoint;
		}

		/// <exception cref="System.IO.IOException"></exception>
		internal void Init()
		{
            _out.Write(Encoding.UTF8.GetBytes(Protocols.ClientBinary));
            _out.Write(Encoding.UTF8.GetBytes(ClientTypes.Csharp));
			_out.Flush();
		}

		/// <exception cref="System.IO.IOException"></exception>
		public bool Write(Data data)
		{
			data.WriteData(_out);
			_out.Flush();
			return true;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public Data Read()
		{
			Data data = new Data();
			data.ReadData(_in);
			lastRead = Clock.CurrentTimeMillis();
			return data;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void Release()
		{
			_out.Close();
			_in.Close();
			GetSocket().Close();
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void Close()
		{
			Release();
		}

		public int GetId()
		{
			return id;
		}

		public long GetLastReadTime()
		{
			return lastRead;
		}

		public override string ToString()
		{
            return "Connection [" + _endpoint + " -> " + GetLocalSocketAddress() + "]";
		}

	    public void Dispose()
	    {
	        clientSocket.Dispose();
	    }

	    public void SetRemoteEndpoint(Address address)
		{
			this._endpoint = address;
		}

		public IPEndPoint GetLocalSocketAddress()
		{
            return (IPEndPoint)clientSocket.LocalEndPoint;
		}
	}
}
