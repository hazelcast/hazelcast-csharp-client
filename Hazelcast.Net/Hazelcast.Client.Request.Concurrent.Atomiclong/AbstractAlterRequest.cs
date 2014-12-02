using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    internal abstract class AbstractAlterRequest : ClientRequest
    {
        protected internal IData function;
        protected internal string name;

        protected AbstractAlterRequest(string name, IData function)
        {
            this.name = name;
            this.function = function;
        }

        public override int GetFactoryId()
        {
            return AtomicLongPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(function);
        }
    }
}