using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class CompareAndRemoveRequest : QueueRequest
    {
        private readonly ICollection<IData> dataList;
        internal bool retain;

        public CompareAndRemoveRequest(string name, ICollection<IData> dataList, bool retain) : base(name)
        {
            this.dataList = dataList;
            this.retain = retain;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.CompareAndRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteBoolean("r", retain);
            writer.WriteInt("s", dataList.Count);
            IObjectDataOutput output = writer.GetRawDataOutput();
            foreach (IData data in dataList)
            {
                output.WriteData(data);
            }
        }
    }
}