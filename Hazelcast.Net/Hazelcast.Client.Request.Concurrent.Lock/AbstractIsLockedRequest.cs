using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal abstract class AbstractIsLockedRequest : ClientRequest
    {
        private readonly long threadId;
        protected internal IData key;

        protected internal AbstractIsLockedRequest(IData key, long threadId = 0)
        {
            this.key = key;
            this.threadId = threadId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("tid", threadId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(key);
        }
    }
}