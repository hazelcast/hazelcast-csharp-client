using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Multimap
{
    internal abstract class MultiMapKeyBasedRequest : MultiMapRequest
    {
        internal IData key;

        protected internal MultiMapKeyBasedRequest(string name, IData key) : base(name)
        {
            this.key = key;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}