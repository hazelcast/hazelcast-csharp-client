using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class KeyBasedContainsRequest : MultiMapKeyBasedRequest
    {
        internal Data value;
        public KeyBasedContainsRequest(string name, Data key, Data value)
            : base(name, key)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.KeyBasedContains;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IOUtil.WriteNullableData(writer.GetRawDataOutput(), value);
        }

    }
}