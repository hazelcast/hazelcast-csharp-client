using System;
using System.Text;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client
{
    internal sealed class GenericError :Exception, IPortable
    {
        private string name;
        private string message;
        private string details;
        private int type;

        public string Name
        {
            get { return name; }
        }

        public string Message
        {
            get { return message; }
        }

        public string Details
        {
            get { return details; }
        }

        public int Type
        {
            get { return type; }
        }

        public GenericError()
        {
        }

        public GenericError(string name, string message, string details, int type)
        {
            this.name = name;
            this.message = message;
            this.details = details;
            this.type = type;
        }

        public int GetFactoryId()
        {
            return ClientPortableHook.Id;
        }

        public int GetClassId()
        {
            return ClientPortableHook.GenericError;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("m", message);
            writer.WriteUTF("d", details);
            writer.WriteInt("t", type);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            message = reader.ReadUTF("m");
            details = reader.ReadUTF("d");
            type = reader.ReadInt("t");
        }

        public override string ToString()
        {
            var sb = new StringBuilder("GenericError{");
            sb.Append("name='").Append(name).Append('\'');
            sb.Append(", message='").Append(message).Append('\'');
            sb.Append(", type=").Append(type);
            sb.Append('}');
            return sb.ToString();
        }
    }
}