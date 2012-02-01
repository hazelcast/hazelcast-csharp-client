using System;

namespace Hazelcast.Client
{
	public class ClientConfig
	{
		private GroupConfig groupConfig = new GroupConfig();
		private TcpIpConfig tcpIpConfig = new TcpIpConfig();
		private int connectionTimeout = 300000;
		
		private Hazelcast.IO.ITypeConverter typeConverter;
		
		private int initialConnectionAttemptLimit = 1;
		private int reconnectionAttemptLimit = 1;	
		private int reConnectionTimeOut = 5000;

		public ClientConfig ()
		{
		}
		
		
		public GroupConfig GroupConfig {
			get {
				return this.groupConfig;
			}
			set {
				groupConfig = value;
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

		public TcpIpConfig TcpIpConfig {
			get {
				return this.tcpIpConfig;
			}
			set {
				tcpIpConfig = value;
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
		
	}
}

