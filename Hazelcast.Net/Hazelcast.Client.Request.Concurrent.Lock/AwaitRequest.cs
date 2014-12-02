using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal class AwaitRequest : ClientRequest
    {
        private readonly string conditionId;
        private readonly string name;
        private readonly IObjectNamespace ns;
        private readonly long threadId;
        private readonly long timeout;

        public AwaitRequest(IObjectNamespace ns, string name, long timeout, long threadId, string conditionId)
        {
            this.ns = ns;
            this.name = name;
            this.timeout = timeout;
            this.threadId = threadId;
            this.conditionId = conditionId;
        }

        public override int GetFactoryId()
        {
            return LockPortableHook.FactoryId;
        }

        public override int GetClassId()
        {
            return LockPortableHook.ConditionAwait;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteLong("tout", timeout);
            writer.WriteLong("tid", threadId);
            writer.WriteUTF("cid", conditionId);
            IObjectDataOutput @out = writer.GetRawDataOutput();
            ns.WriteData(@out);
        }
    }
}