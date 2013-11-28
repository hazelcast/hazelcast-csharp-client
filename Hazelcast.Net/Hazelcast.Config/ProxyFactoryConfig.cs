

namespace Hazelcast.Config
{
	
	public class ProxyFactoryConfig
	{
		private string service;

		private string className;

		public ProxyFactoryConfig()
		{
		}

		public ProxyFactoryConfig(string className, string service)
		{
			this.className = className;
			this.service = service;
		}

		public virtual string GetClassName()
		{
			return className;
		}

		public virtual void SetClassName(string className)
		{
			this.className = className;
		}

		public virtual string GetService()
		{
			return service;
		}

		public virtual void SetService(string service)
		{
			this.service = service;
		}
	}
}
