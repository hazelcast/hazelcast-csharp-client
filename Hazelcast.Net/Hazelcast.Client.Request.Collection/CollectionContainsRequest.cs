using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionContainsRequest : CollectionRequest
    {
        private ICollection<Data> valueSet;


        public CollectionContainsRequest(string name, ICollection<Data> valueSet) : base(name)
        {
            this.valueSet = valueSet;
        }

        public CollectionContainsRequest(string name, Data value) : base(name)
        {
            valueSet = new HashSet<Data> {value};
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionContains;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(valueSet.Count);
            foreach (Data value in valueSet)
            {
                value.WriteData(output);
            }
        }
    }
}