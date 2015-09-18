using System;

namespace Hazelcast.Security
{
    /// <summary>
    ///     Abstract implementation of
    ///     <see cref="ICredentials">ICredentials</see>
    /// </summary>
    [Serializable]
    internal abstract class AbstractCredentials : ICredentials
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

        public override int GetHashCode()
        {
            var prime = 31;
            var result = 1;
            result = prime*result + ((principal == null) ? 0 : principal.GetHashCode());
            return result;
        }

        public virtual void SetPrincipal(string principal)
        {
            this.principal = principal;
        }
    }
}