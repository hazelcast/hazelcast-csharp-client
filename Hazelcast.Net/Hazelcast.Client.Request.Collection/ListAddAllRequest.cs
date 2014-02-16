using System.Collections.Generic;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public class ListAddAllRequest : CollectionAddAllRequest
    {
        private int index;

        public ListAddAllRequest()
        {
        }

        public ListAddAllRequest(string name, IList<Data> valueList, int index) : base(name, valueList)
        {
            this.index = index;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListAddAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("i", index);
            base.WritePortable(writer);
        }

    }
}