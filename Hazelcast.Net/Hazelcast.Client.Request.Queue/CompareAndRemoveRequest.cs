using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class CompareAndRemoveRequest : QueueRequest
    {
        private ICollection<Data> dataList;

        internal bool retain;

        public CompareAndRemoveRequest()
        {
        }

        public CompareAndRemoveRequest(string name, ICollection<Data> dataList, bool retain) : base(name)
        {
            this.dataList = dataList;
            this.retain = retain;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.CompareAndRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteBoolean("r", retain);
            writer.WriteInt("s", dataList.Count);
            IObjectDataOutput output = writer.GetRawDataOutput();
            foreach (Data data in dataList)
            {
                data.WriteData(output);
            }
        }

    }
}