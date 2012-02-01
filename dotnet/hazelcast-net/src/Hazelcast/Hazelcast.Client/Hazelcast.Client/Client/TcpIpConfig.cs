using System;
using System.Collections.Generic;
using Hazelcast.Core;

namespace Hazelcast.Client
{
	public class TcpIpConfig
	{
		private List<String> members = new List<String>();
		
		private List<Address> addresses = new List<Address>();
		
		
		public TcpIpConfig ()
		{
		}

		public List<Address> Addresses {
			get {
				return this.addresses;
			}
		}

		public List<String> Members {
			get {
				return this.members;
			}
		}
		
		public TcpIpConfig addMember(String member) {
        	this.members.Add(member);
	        return this;
	    }
		public TcpIpConfig clear() {
	        members.Clear();
	        addresses.Clear();
	        return this;
	    }
	
	    public TcpIpConfig addAddress(Address address) {
	        addresses.Add(address);
	        return this;
	    }
		
		
	}
}

