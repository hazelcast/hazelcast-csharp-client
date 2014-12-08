using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapReplaceIfSameRequest : MapPutRequest
    {
        private readonly IData testValue;

        public MapReplaceIfSameRequest(string name, IData key, IData testValue, IData value, long threadId)
            : base(name, key, value, threadId)
        {
            this.testValue = testValue;
        }

        public override int GetClassId()
        {
            return MapPortableHook.ReplaceIfSame;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(testValue);
        }
    }
}