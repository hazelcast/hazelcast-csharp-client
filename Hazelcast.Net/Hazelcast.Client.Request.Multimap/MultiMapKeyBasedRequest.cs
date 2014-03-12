using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Multimap
{
    internal abstract class MultiMapKeyBasedRequest : MultiMapRequest
    {
        internal Data key;


        protected internal MultiMapKeyBasedRequest(string name, Data key) : base(name)
        {
            this.key = key;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }


    }
}