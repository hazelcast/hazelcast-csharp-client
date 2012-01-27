using System;
using System.Collections.Generic;

namespace Hazelcast.Client
{
	
	public class ClientPropertyName{
		
		public static ClientPropertyName GROUP_NAME = new ClientPropertyName("hazelcast.client.group.name", null);
        public static ClientPropertyName GROUP_PASSWORD = new ClientPropertyName("hazelcast.client.group.password", null);
        public static ClientPropertyName INIT_CONNECTION_ATTEMPTS_LIMIT = new ClientPropertyName("hazelcast.client.init.connection.attempts.limit", "5");
        public static ClientPropertyName RECONNECTION_ATTEMPTS_LIMIT = new ClientPropertyName("hazelcast.client.reconnection.attempts.limit", "5");
        public static ClientPropertyName CONNECTION_TIMEOUT = new ClientPropertyName("hazelcast.client.connection.timeout", "300000");
        public static ClientPropertyName RECONNECTION_TIMEOUT = new ClientPropertyName("hazelcast.client.reconnection.timeout", "5000");
		
		
		private String name;
		
		private String defaultValue;
		
		
		public ClientPropertyName(String name, String defaultValue){
			this.name = name;
			this.defaultValue = defaultValue;
		}

		public String DefaultValue {
			get {
				return this.defaultValue;
			}
			set {
				defaultValue = value;
			}
		}		
		
		
		public String Name {
			get {
				return this.name;
			}
			set {
				name = value;
			}
		}		
		public static ClientPropertyName fromValue(String value){
			return null;
		}
		
	}
	public class ClientProperties
	{
	
	    private IDictionary<ClientPropertyName, String> properties;
	
	    public ClientProperties() {
	        this.properties = new Dictionary<ClientPropertyName, String>();
	    }
	
	    public IDictionary<ClientPropertyName, String> getProperties() {
	        return this.properties;
	    }
	
	    public ClientProperties setProperties(IDictionary<ClientPropertyName, String> properties) {
	        foreach (ClientPropertyName  key in properties.Keys) {
	            setPropertyValue(key, properties[key]);
	        }
	        return this;
	    }
	    
	    public ClientProperties setPropertyValue(String name, String value) {
	        return setPropertyValue(ClientPropertyName.fromValue(name), value);
	    }
	
	    public ClientProperties setPropertyValue( ClientPropertyName name, String value) {
	        this.properties[name] = value;
	        return this;
	    }
	
	    public String getProperty(String name) {
	        return getProperty(ClientPropertyName.fromValue(name));
	    }
	
	    public String getProperty(ClientPropertyName name) {
	        String str = this.properties[name];
	        if (str == null) {
	            str = name.DefaultValue;
	        }
	        if (str == null) {
	            throw new Exception("property " + name.Name + " is null");
	        }
	        return str;
	    }
	
	    public int getInteger(ClientPropertyName name) {
	        return int.Parse(getProperty(name));
	    }
	
	    public long getLong(ClientPropertyName name) {
	        return long.Parse(getProperty(name));
	    }
	
	    public static ClientProperties createBaseClientProperties(String groupName, String groupPassword) {
	        ClientProperties clientProperties = new ClientProperties();
	        clientProperties.setPropertyValue(ClientPropertyName.GROUP_NAME, groupName);
	        clientProperties.setPropertyValue(ClientPropertyName.GROUP_PASSWORD, groupPassword);
	        return clientProperties;
	    }
	}
}

