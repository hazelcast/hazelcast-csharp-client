using System.Text;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client
{
    internal sealed class ClientPrincipal 
    {
        private string ownerUuid;
        private string uuid;

        public ClientPrincipal()
        {
        }

        public ClientPrincipal(string uuid, string ownerUuid)
        {
            this.uuid = uuid;
            this.ownerUuid = ownerUuid;
        }

        public string GetUuid()
        {
            return uuid;
        }

        public string GetOwnerUuid()
        {
            return ownerUuid;
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
            var that = (ClientPrincipal) o;
            if (ownerUuid != null ? !ownerUuid.Equals(that.ownerUuid) : that.ownerUuid != null)
            {
                return false;
            }
            if (uuid != null ? !uuid.Equals(that.uuid) : that.uuid != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = uuid != null ? uuid.GetHashCode() : 0;
            result = 31*result + (ownerUuid != null ? ownerUuid.GetHashCode() : 0);
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("ClientPrincipal{");
            sb.Append("uuid='").Append(uuid).Append('\'');
            sb.Append(", ownerUuid='").Append(ownerUuid).Append('\'');
            sb.Append('}');
            return sb.ToString();
        }
    }
}