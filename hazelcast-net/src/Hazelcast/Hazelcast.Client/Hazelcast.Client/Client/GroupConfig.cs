using System;
using Hazelcast.IO;

namespace Hazelcast.Client
{
	public class GroupConfig
	{
		public static String DEFAULT_GROUP_PASSWORD = "dev-pass";
	    public static String DEFAULT_GROUP_NAME = "dev";
	
	    private String name = DEFAULT_GROUP_NAME;
	    private String password = DEFAULT_GROUP_PASSWORD;
		
	    public GroupConfig() {
	    }
	
	    public GroupConfig(String name) {
	        Name = name;
	    }
	
	    public GroupConfig(String name, String password) {
	        Name = name;
	        Password = password;
	    }
		
			public String Name {
			get {
				return this.name;
			}
			set {
				name = value;
			}
		}

		public String Password {
			get {
				return this.password;
			}
			set {
				password = value;
			}
		}	
		
	}
}

