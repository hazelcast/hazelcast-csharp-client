using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Hazelcast.Core;
using Hazelcast.Impl;

namespace Hazelcast.Client
{
	public class Connection
	{
		private static int BUFFER_SIZE = 16 << 10; // 32k
	    private volatile TcpClient tcpClient;
	    private IPEndPoint address;
	    private int id;
	    //private DataOutputStream dos;
	    //private DataInputStream dis;
		private BufferedStream bs = null;
		
	    bool headersWritten = false;
	    bool headerRead = false;
	
	    /**
	     * Creates the Socket to the given host and port
	     *
	     * @param host ip address of the host
	     * @param port port of the host
	     * @throws UnknownHostException
	     * @throws IOException
	     */
	    public Connection(String host, int port, int id) {
			IPHostEntry ipHostEntry = Dns.GetHostEntry(host);
	        initiate(new IPEndPoint(ipHostEntry.AddressList[0], port), id);
	    }
	
		public Connection(IPEndPoint address, int id) {
			initiate(address, id);
		}
	    private void initiate(IPEndPoint address, int id) {
	        this.id = id;
	        this.address = address;
	        try {
	            IPEndPoint isa = new IPEndPoint(address.Address, address.Port);
	            TcpClient tcp = new TcpClient(isa);
				tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				
				LingerOption lingerOption = new LingerOption (true, 5);
				tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
	            this.tcpClient = tcp;
				
				bs = new BufferedStream(tcpClient.GetStream(), BUFFER_SIZE);
	        } catch (Exception e) {
	            throw new Exception(e.ToString());
	        }
	    }
	
	    public IPEndPoint getAddress() {
	        return address;
	    }
	
	    public int getVersion() {
	        return id;
	    }
	
	    public void close() {
	        bs.Close();
			tcpClient.Close();
	    }
	
	    
	    public override String ToString() {
	        return "Connection [" + id + "]" + " [" + address + " -> " + tcpClient.Client.LocalEndPoint.ToString()+ "]";
	    }
	
	    //public DataOutputStream getOutputStream() {
	    //    return dos;
	    //}
		
		public Stream getNetworkStream(){
			return bs;
		}
	
	    //public DataInputStream getInputStream() {
	    //    return dis;
	    //}
	
	    public Member getMember() {
	        return new MemberImpl(new Address(address));
	    }
	}
}

