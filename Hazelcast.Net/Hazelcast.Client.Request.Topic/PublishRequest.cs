using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Topic
{
    public class PublishRequest : ClientRequest
    {
        private Data message;
        internal string name;

        public PublishRequest()
        {
        }

        public PublishRequest(string name, Data message)
        {
            this.name = name;
            this.message = message;
        }

        public override int GetFactoryId()
        {
            return TopicPortableHook.FId;
        }

        public override int GetClassId()
        {
            return TopicPortableHook.Publish;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            message.WriteData(output);
        }

    }
}