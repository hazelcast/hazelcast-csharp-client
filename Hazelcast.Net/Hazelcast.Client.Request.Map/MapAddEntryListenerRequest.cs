using Hazelcast.Client.Request.Base;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    internal class MapAddEntryListenerRequest<K, V> : ClientRequest
    {
        private readonly bool includeValue;
        private readonly IData key;
        private readonly string name;

        private readonly IPredicate<K,V> predicate;

        public MapAddEntryListenerRequest(string name, bool includeValue)
        {
            this.name = name;
            this.includeValue = includeValue;
        }

        public MapAddEntryListenerRequest(string name, IData key, bool includeValue) : this(name, includeValue)
        {
            this.key = key;
        }

        public MapAddEntryListenerRequest(string name, IData key, bool includeValue, IPredicate<K, V> predicate)
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
        public override void Write(IPortableWriter writer)
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
                    output.WriteData(key);
                }
            }
            else
            {
                writer.WriteBoolean("pre", true);
                IObjectDataOutput output = writer.GetRawDataOutput();
                output.WriteObject(predicate);
                if (hasKey)
                {
                    output.WriteData(key);
                }
            }
        }
    }
}