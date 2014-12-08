using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    internal class AddAllRequest : QueueRequest
    {
        private readonly ICollection<IData> dataList;

        public AddAllRequest(string name, ICollection<IData> dataList) : base(name)
        {
            this.dataList = dataList;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.AddAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteInt("s", dataList.Count);
            IObjectDataOutput output = writer.GetRawDataOutput();
            foreach (IData data in dataList)
            {
                output.WriteData(data);
            }
        }
    }
}