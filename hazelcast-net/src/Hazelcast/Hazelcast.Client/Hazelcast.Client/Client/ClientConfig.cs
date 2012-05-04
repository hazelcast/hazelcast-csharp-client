using System;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using Hazelcast.Security;

namespace Hazelcast.Client
{
	public class ClientConfig
	{
		private GroupConfig groupConfig = new GroupConfig();
		List<IPEndPoint> addressList = new List<IPEndPoint>();
		private int connectionTimeout = 300000;
		
		private Hazelcast.IO.ITypeConverter typeConverter;
		
		private int initialConnectionAttemptLimit = 1;
		private int reconnectionAttemptLimit = 1;	
		private int reConnectionTimeOut = 5000;
		private bool shuffle;
		private bool automatic;
		
		private Credentials credentials;

	
		public ClientConfig ()
		{
		}
		
		public List<IPEndPoint> AddressList{
			get{return this.addressList;}
		}
		
		
		public GroupConfig GroupConfig {
			get {
				return this.groupConfig;
			}
			set {
				groupConfig = value;
			}
		}
		
		public Credentials Credentials {
			get {
				return this.credentials;
			}
			set {
				credentials = value;
			}
		}
		
		public int ConnectionTimeout {
			get {
				return this.connectionTimeout;
			}
			set {
				connectionTimeout = value;
			}
		}

		public bool Automatic {
			get {
				return this.automatic;
			}
			set {
				automatic = value;
			}
		}

		public bool Shuffle {
			get {
				return this.shuffle;
			}
			set {
				shuffle = value;
			}
		}		
		
		
		public int InitialConnectionAttemptLimit {
			get {
				return this.initialConnectionAttemptLimit;
			}
			set {
				initialConnectionAttemptLimit = value;
			}
		}

		public int ReconnectionAttemptLimit {
			get {
				return this.reconnectionAttemptLimit;
			}
			set {
				reconnectionAttemptLimit = value;
			}
		}

		public int ReConnectionTimeOut {
			get {
				return this.reConnectionTimeOut;
			}
			set {
				reConnectionTimeOut = value;
			}
		}	
		
		public Hazelcast.IO.ITypeConverter TypeConverter {
			get {
				return this.typeConverter;
			}
			set {
				typeConverter = value;
			}
		}
	
	    public ClientConfig addAddress(String address) {
	        this.addressList.Add(parse(address));
	        return this;
	    }

	   	private static IPEndPoint parse(String address) {
	        String[] separated = address.Split(':');
	        int port = (separated.Length > 1) ? int.Parse(separated[1]) : 5701;
			IPEndPoint iPEndPoint = null;
			try
			{
				iPEndPoint = CreateIPEndPoint(address);
			}catch(FormatException ex){
				iPEndPoint = new IPEndPoint(Dns.GetHostEntry(separated[0]).AddressList[0], port);	
			}
			 
			return iPEndPoint;
	    }
		
		public static IPEndPoint CreateIPEndPoint(string endPoint)
		{
		    string[] ep = endPoint.Split(':');
		    if(ep.Length != 2) throw new FormatException("Invalid endpoint format");
		    IPAddress ip;
		    if(!IPAddress.TryParse(ep[0], out ip))
		    {
		        throw new FormatException("Invalid ip-adress");
		    }
		    int port;
		    if(!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
		    {
		        throw new FormatException("Invalid port");
		    }
		    return new IPEndPoint(ip, port);
		}
		
	}
}

