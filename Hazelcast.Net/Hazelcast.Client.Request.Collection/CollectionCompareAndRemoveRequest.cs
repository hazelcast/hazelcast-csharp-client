using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionCompareAndRemoveRequest : CollectionRequest
    {
        private bool retain;
        private ICollection<Data> valueSet;

        public CollectionCompareAndRemoveRequest()
        {
        }

        public CollectionCompareAndRemoveRequest(string name, ICollection<Data> valueSet, bool retain) : base(name)
        {
            this.valueSet = valueSet;
            this.retain = retain;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionCompareAndRemove;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteBoolean("r", retain);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(valueSet.Count);
            foreach (Data value in valueSet)
            {
                value.WriteData(output);
            }
        }

    }
}