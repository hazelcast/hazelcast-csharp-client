using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapReplaceIfSameRequest : MapPutRequest
    {
        private Data testValue;

        public MapReplaceIfSameRequest()
        {
        }

        public MapReplaceIfSameRequest(string name, Data key, Data testValue, Data value, long threadId)
            : base(name, key, value, threadId)
        {
            this.testValue = testValue;
        }

        public override int GetClassId()
        {
            return MapPortableHook.ReplaceIfSame;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            testValue.WriteData(output);
        }

    }
}