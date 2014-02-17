using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionAddAllRequest : CollectionRequest
    {
        protected internal IList<Data> valueList;

        public CollectionAddAllRequest()
        {
        }

        public CollectionAddAllRequest(string name, IList<Data> valueList) : base(name)
        {
            this.valueList = valueList;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionAddAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(valueList.Count);
            foreach (Data value in valueList)
            {
                value.WriteData(output);
            }
        }

    }
}