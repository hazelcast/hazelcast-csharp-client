using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.Client.Request.Map
{
    internal sealed class MapQueryRequest<K, V> : AbstractMapQueryRequest
    {
        private IPredicate<K, V> predicate;

        public MapQueryRequest()
        {
        }

        public MapQueryRequest(string name, IPredicate<K, V> predicate, IterationType iterationType)
            : base(name, iterationType)
        {
            this.predicate = predicate;
        }

        public override int GetClassId()
        {
            return MapPortableHook.Query;
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal override void WritePortableInner(IPortableWriter writer)
        {
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteObject(predicate);
        }

    }
}