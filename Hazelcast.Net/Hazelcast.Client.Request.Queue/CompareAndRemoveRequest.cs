using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class CompareAndRemoveRequest : QueueRequest
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

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            retain = reader.ReadBoolean("r");
            int size = reader.ReadInt("s");
            IObjectDataInput input = reader.GetRawDataInput();
            dataList = new List<Data>(size);
            for (int i = 0; i < size; i++)
            {
                var data = new Data();
                data.ReadData(input);
                dataList.Add(data);
            }
        }
    }
}