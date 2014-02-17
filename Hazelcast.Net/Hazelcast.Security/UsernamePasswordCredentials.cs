using System;
using System.Text;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Security
{
    /// <summary>
    ///     Simple implementation of
    ///     <see cref="ICredentials">ICredentials</see>
    ///     using
    ///     username and password as security attributes.
    /// </summary>
    [Serializable]
    internal class UsernamePasswordCredentials : AbstractCredentials
    {
        private byte[] password;

        public UsernamePasswordCredentials()
        {
        }

        public UsernamePasswordCredentials(string username, string password) : base(username)
        {
            this.password = Encoding.UTF8.GetBytes(password);
        }

        public virtual string GetUsername()
        {
            return GetPrincipal();
        }

        public virtual byte[] GetRawPassword()
        {
            return password;
        }

        public virtual string GetPassword()
        {
            return password == null ? string.Empty : Encoding.UTF8.GetString(password);
        }

        public virtual void SetUsername(string username)
        {
            SetPrincipal(username);
        }

        public virtual void SetPassword(string password)
        {
            this.password = Encoding.UTF8.GetBytes(password);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal override void WritePortableInternal(IPortableWriter writer)
        {
            writer.WriteByteArray("pwd", password);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal override void ReadPortableInternal(IPortableReader reader)
        {
            password = reader.ReadByteArray("pwd");
        }

        public override int GetFactoryId()
        {
            return SpiPortableHook.Id;
        }

        public override int GetClassId()
        {
            return SpiPortableHook.UsernamePwdCred;
        }

        public override string ToString()
        {
            return "UsernamePasswordCredentials [username=" + GetUsername() + "]";
        }
    }
}