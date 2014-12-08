using System.Collections.Generic;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapGetAllRequest : ClientRequest, IRetryableRequest
    {
        private readonly ICollection<IData> keys;
        protected internal string name;

        public MapGetAllRequest(string name, ICollection<IData> keys)
        {
            this.name = name;
            this.keys = keys;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.GetAll;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("size", keys.Count);
            if (keys.Count > 0)
            {
                IObjectDataOutput output = writer.GetRawDataOutput();
                foreach (IData key in keys)
                {
                    output.WriteData(key);
                }
            }
        }
    }
}