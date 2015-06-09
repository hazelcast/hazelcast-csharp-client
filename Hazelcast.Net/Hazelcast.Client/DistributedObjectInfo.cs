using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client
{
    internal class DistributedObjectInfo
    {
        private string name;
        private string serviceName;

        internal DistributedObjectInfo()
        {
        }

        public DistributedObjectInfo(string serviceName, string name)
        {
            this.name = name;
            this.serviceName = serviceName;
        }

        //REQUIRED

        public virtual string GetServiceName()
        {
            return serviceName;
        }

        public virtual string GetName()
        {
            return name;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            var that = (DistributedObjectInfo) o;
            if (name != null ? !name.Equals(that.name) : that.name != null)
            {
                return false;
            }
            if (serviceName != null ? !serviceName.Equals(that.serviceName) : that.serviceName != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = serviceName != null ? serviceName.GetHashCode() : 0;
            result = 31*result + (name != null ? name.GetHashCode() : 0);
            return result;
        }
    }
}