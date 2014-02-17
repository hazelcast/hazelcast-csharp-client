using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapAddEntryListenerRequest<K, V> : ClientRequest
    {
        private IPredicate<K, V> predicate;

        private bool includeValue;
        private Data key;
        private string name;

        public MapAddEntryListenerRequest()
        {
        }

        public MapAddEntryListenerRequest(string name, bool includeValue)
        {
            this.name = name;
            this.includeValue = includeValue;
        }

        public MapAddEntryListenerRequest(string name, Data key, bool includeValue) : this(name, includeValue)
        {
            this.key = key;
        }

        public MapAddEntryListenerRequest(string name, Data key, bool includeValue, IPredicate<K, V> predicate)
            : this(name, key, includeValue)
        {
            this.predicate = predicate;
        }

        public override int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public override int GetClassId()
        {
            return MapPortableHook.AddEntryListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteBoolean("i", includeValue);
            bool hasKey = key != null;
            writer.WriteBoolean("key", hasKey);
            if (predicate == null)
            {
                writer.WriteBoolean("pre", false);
                if (hasKey)
                {
                    IObjectDataOutput output = writer.GetRawDataOutput();
                    key.WriteData(output);
                }
            }
            else
            {
                writer.WriteBoolean("pre", true);
                IObjectDataOutput output = writer.GetRawDataOutput();
                output.WriteObject(predicate);
                if (hasKey)
                {
                    key.WriteData(output);
                }
            }
        }

   
    }
}