using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionCompareAndRemoveRequest : CollectionRequest
    {
        private readonly bool retain;
        private readonly ICollection<IData> valueSet;

        public CollectionCompareAndRemoveRequest(string name, ICollection<IData> valueSet, bool retain) : base(name)
        {
            this.valueSet = valueSet;
            this.retain = retain;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionCompareAndRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            writer.WriteBoolean("r", retain);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(valueSet.Count);
            foreach (IData value in valueSet)
            {
                output.WriteData(value);
            }
        }
    }
}