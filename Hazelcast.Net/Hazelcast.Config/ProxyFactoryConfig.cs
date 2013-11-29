namespace Hazelcast.Config
{
    public class ProxyFactoryConfig
    {
        private string className;
        private string service;

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