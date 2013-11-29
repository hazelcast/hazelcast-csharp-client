using System.Text;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Config
{
    public class GlobalSerializerConfig
    {
        private string className;

        private ISerializer implementation;

        public GlobalSerializerConfig() : base()
        {
        }

        public virtual string GetClassName()
        {
            return className;
        }

        public virtual GlobalSerializerConfig SetClassName(string className)
        {
            this.className = className;
            return this;
        }

        public virtual ISerializer GetImplementation()
        {
            return implementation;
        }

        //public virtual GlobalSerializerConfig SetImplementation(IByteArraySerializer<> implementation)
        //{
        //    this.implementation = implementation;
        //    return this;
        //}

        //public virtual GlobalSerializerConfig SetImplementation(IStreamSerializer<> implementation)
        //{
        //    this.implementation = implementation;
        //    return this;
        //}

        public override string ToString()
        {
            var sb = new StringBuilder("GlobalSerializerConfig{");
            sb.Append("className='").Append(className).Append('\'');
            sb.Append(", implementation=").Append(implementation);
            sb.Append('}');
            return sb.ToString();
        }
    }
}