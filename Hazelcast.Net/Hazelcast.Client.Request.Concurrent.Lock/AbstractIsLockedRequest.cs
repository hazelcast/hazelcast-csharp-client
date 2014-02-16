using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    public abstract class AbstractIsLockedRequest : ClientRequest
    {
        protected internal Data key;

        private long threadId;

        protected AbstractIsLockedRequest()
        {
        }

        protected AbstractIsLockedRequest(Data key, long threadId = 0)
        {
            this.key = key;
            this.threadId = threadId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("tid", threadId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }


    }
}