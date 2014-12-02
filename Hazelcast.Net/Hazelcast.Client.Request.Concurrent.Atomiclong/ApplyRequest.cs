using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Concurrent.Atomiclong
{
    internal class ApplyRequest : ReadRequest
    {
        private readonly IData function;

        public ApplyRequest(string name, IData function) : base(name)
        {
            this.function = function;
        }

        public override int GetClassId()
        {
            return AtomicLongPortableHook.Apply;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(function);
        }
    }
}