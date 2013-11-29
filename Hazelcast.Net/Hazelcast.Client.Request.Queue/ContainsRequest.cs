using System.Collections.Generic;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    public class ContainsRequest : QueueRequest, IRetryableRequest
    {
        internal ICollection<Data> dataList;

        public ContainsRequest()
        {
        }

        public ContainsRequest(string name, ICollection<Data> dataList) : base(name)
        {
            this.dataList = dataList;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.Contains;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
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