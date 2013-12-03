using System;
using System.Net.Sockets;
using System.Net;
using Hazelcast.Security;
using Hazelcast.Core;
using Hazelcast.Client.IO;
using Hazelcast.Cluster;

namespace Hazelcast.Client
{
	public class DefaultClientBinder : ClientBinder
	{
		private HazelcastClient client;
	    //private ILogger logger = Logger.getLogger(getClass().getName());
	
	    public DefaultClientBinder(HazelcastClient client) {
	        this.client = client;
	    }
	
	    public void bind(Connection connection, Credentials credentials){
	        //logger.log(Level.FINEST, connection + " -> "
	        //        + connection.getAddress().getHostName() + ":" + connection.getSocket().getLocalPort());
	        auth(connection, credentials);
			//IPEndPoint iPEndPoint = new IPEndPoint(connection.getAddress().Address, ((IPEndPoint)connection.getSocket().LocalEndPoint).Port);
	        //Bind b = new Bind(new Address(iPEndPoint));
	        //Packet bind = new Packet();
	        //bind.set("remotelyProcess", ClusterOperation.REMOTELY_PROCESS, null, IOUtil.toByte(b));
	        //write(connection, bind);
	    }
	
	    void auth(Connection connection, Credentials credentials){
			Packet auth = new Packet();
	        auth.set("", ClusterOperation.CLIENT_AUTHENTICATE, new byte[0], IOUtil.toByte(credentials));
	        Packet packet = writeAndRead(connection, auth);
			Object response = IOUtil.toObject(packet.value);
	        //logger.log(Level.FINEST, "auth responce:" + response);
			//TODO Exception handling
	        if (response is Exception) {
	            throw new Exception(response.ToString());
	        }
	        if (!true.Equals(response)) {
	            throw new Exception("Authentication Exception! Client [" + connection + "] has failed authentication");
	        }
	    }
	
	    Packet writeAndRead(Connection connection, Packet packet) {
	        write(connection, packet);
	        return client.InThread.readPacket(connection);
	    }
	
	    void write(Connection connection, Packet packet) {
	        OutThread.write(connection, packet);
	    }
	}
}

