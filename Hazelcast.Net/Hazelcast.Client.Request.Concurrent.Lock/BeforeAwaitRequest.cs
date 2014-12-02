using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal class BeforeAwaitRequest : ClientRequest
    {
        private readonly string conditionId;
        private readonly IData key;
        private readonly IObjectNamespace ns;
        private readonly long threadId;

        public BeforeAwaitRequest(IObjectNamespace ns, long threadId, string conditionId, IData key)
        {
            this.ns = ns;
            this.threadId = threadId;
            this.conditionId = conditionId;
            this.key = key;
        }

        public override int GetFactoryId()
        {
            return LockPortableHook.FactoryId;
        }

        public override int GetClassId()
        {
            return LockPortableHook.ConditionBeforeAwait;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteLong("tid", threadId);
            writer.WriteUTF("cid", conditionId);
            IObjectDataOutput output = writer.GetRawDataOutput();
            ns.WriteData(output);
            output.WriteData(key);
        }
    }
}