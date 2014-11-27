using System.Collections.Generic;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal class ListAddAllRequest : CollectionAddAllRequest
    {
        private readonly int index;

        public ListAddAllRequest(string name, IList<IData> valueList, int index) : base(name, valueList)
        {
            this.index = index;
        }

        public override int GetClassId()
        {
            return CollectionPortableHook.ListAddAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteInt("i", index);
            base.Write(writer);
        }
    }
}