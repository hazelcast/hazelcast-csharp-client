using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    public abstract class AbstractLockRequest : IPortable
    {
        protected internal Data key;

        private int threadId;

        private long timeout = -1;
        private long ttl = -1;

        public AbstractLockRequest()
        {
        }

        public AbstractLockRequest(Data key, int threadId)
        {
            this.key = key;
            this.threadId = threadId;
        }

        public AbstractLockRequest(Data key, int threadId, long ttl, long timeout)
        {
            this.key = key;
            this.threadId = threadId;
            this.ttl = ttl;
            this.timeout = timeout;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("tid", threadId);
            writer.WriteLong("ttl", ttl);
            writer.WriteLong("timeout", timeout);
            IObjectDataOutput output = writer.GetRawDataOutput();
            key.WriteData(output);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            threadId = reader.ReadInt("tid");
            ttl = reader.ReadLong("ttl");
            timeout = reader.ReadLong("timeout");
            IObjectDataInput input = reader.GetRawDataInput();
            key = new Data();
            key.ReadData(input);
        }

        public abstract int GetClassId();

        public abstract int GetFactoryId();

        protected internal object GetKey()
        {
            return key;
        }

        public string GetServiceName()
        {
            return ServiceNames.Lock;
        }
    }
}