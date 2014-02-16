using System.Collections.Generic;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapGetAllRequest : ClientRequest, IRetryableRequest
    {
        private readonly ICollection<Data> keys = new HashSet<Data>();
        protected internal string name;

        public MapGetAllRequest()
        {
        }

        public MapGetAllRequest(string name, ICollection<Data> keys)
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
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteInt("size", keys.Count);
            if (keys.Count > 0)
            {
                IObjectDataOutput output = writer.GetRawDataOutput();
                foreach (Data key in keys)
                {
                    key.WriteData(output);
                }
            }
        }

    }
}