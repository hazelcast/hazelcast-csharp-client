using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class CollectionContainsRequest : CollectionRequest
    {
        private readonly ICollection<IData> valueSet;

        public CollectionContainsRequest(string name, ICollection<IData> valueSet) : base(name)
        {
            this.valueSet = valueSet;
        }

        public CollectionContainsRequest(string name, IData value) : base(name)
        {
            valueSet = new HashSet<IData> {value};
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.CollectionContains;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteInt(valueSet.Count);
            foreach (IData value in valueSet)
            {
                output.WriteData(value);
            }
        }
    }
}