using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Lock
{
    internal class SignalRequest : ClientRequest
    {
        private readonly bool all;
        private readonly string conditionId;
        private readonly string name;
        private readonly IObjectNamespace ns;
        private readonly long threadId;

        public SignalRequest(IObjectNamespace ns, string name, long threadId, string conditionId, bool all)
        {
            this.ns = ns;
            this.name = name;
            this.threadId = threadId;
            this.conditionId = conditionId;
            this.all = all;
        }

        public override int GetFactoryId()
        {
            return LockPortableHook.FactoryId;
        }

        public override int GetClassId()
        {
            return LockPortableHook.ConditionSignal;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteUTF("cid", conditionId);
            writer.WriteLong("tid", threadId);
            writer.WriteBoolean("all", all);
            IObjectDataOutput output = writer.GetRawDataOutput();
            ns.WriteData(output);
        }
    }
}