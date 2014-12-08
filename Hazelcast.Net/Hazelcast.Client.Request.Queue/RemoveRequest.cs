using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class RemoveRequest : QueueRequest
    {
        internal IData data;

        public RemoveRequest(string name, IData data) : base(name)
        {
            this.data = data;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Remove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(data);
        }
    }
}