using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Topic
{
    internal class PublishRequest : ClientRequest
    {
        private readonly IData message;
        private readonly string name;

        public PublishRequest(string name, IData message)
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
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(message);
        }
    }
}