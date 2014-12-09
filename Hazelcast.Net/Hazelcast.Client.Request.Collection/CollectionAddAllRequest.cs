using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionAddAllRequest : CollectionRequest
    {
        protected internal IList<IData> valueList;

        public CollectionAddAllRequest(string name, IList<IData> valueList) : base(name)
        {
            this.valueList = valueList;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionAddAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(valueList.Count);
            foreach (IData value in valueList)
            {
                output.WriteData(value);
            }
        }
    }
}