using System;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Security
{
    /// <summary>
    ///     Abstract implementation of
    ///     <see cref="ICredentials">ICredentials</see>
    /// </summary>
    [Serializable]
    public abstract class AbstractCredentials : ICredentials
    {
        private string endpoint;

        private string principal;

        public AbstractCredentials()
        {
        }

        public AbstractCredentials(string principal)
        {
            this.principal = principal;
        }

        public string GetEndpoint()
        {
            return endpoint;
        }

        public void SetEndpoint(string endpoint)
        {
            this.endpoint = endpoint;
        }

        public virtual string GetPrincipal()
        {
            return principal;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("principal", principal);
            writer.WriteUTF("endpoint", endpoint);
            WritePortableInternal(writer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadPortable(IPortableReader reader)
        {
            principal = reader.ReadUTF("principal");
            endpoint = reader.ReadUTF("endpoint");
            ReadPortableInternal(reader);
        }

        public abstract int GetClassId();

        public abstract int GetFactoryId();

        public virtual void SetPrincipal(string principal)
        {
            this.principal = principal;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime*result + ((principal == null) ? 0 : principal.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            var other = (AbstractCredentials) obj;
            if (principal == null)
            {
                if (other.principal != null)
                {
                    return false;
                }
            }
            else
            {
                if (!principal.Equals(other.principal))
                {
                    return false;
                }
            }
            return true;
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void WritePortableInternal(IPortableWriter writer);

        /// <exception cref="System.IO.IOException"></exception>
        protected internal abstract void ReadPortableInternal(IPortableReader reader);
    }
}