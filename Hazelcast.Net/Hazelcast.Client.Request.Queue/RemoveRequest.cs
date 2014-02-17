using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class RemoveRequest : QueueRequest
    {
        internal Data data;

        public RemoveRequest()
        {
        }

        public RemoveRequest(string name, Data data) : base(name)
        {
            this.data = data;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Remove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            data.WriteData(output);
        }

    }
}