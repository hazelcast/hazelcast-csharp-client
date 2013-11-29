using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Map
{
    public class MapAddEntryListenerRequest : IPortable
    {
        private readonly IPredicate<object, object> predicate;

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

        public MapAddEntryListenerRequest(string name, Data key, bool includeValue, IPredicate<object, object> predicate)
            : this(name, key, includeValue)
        {
            this.predicate = predicate;
        }

        public virtual int GetFactoryId()
        {
            return MapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MapPortableHook.AddEntryListener;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
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

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("name");
            includeValue = reader.ReadBoolean("i");
            bool hasKey = reader.ReadBoolean("key");
            if (reader.ReadBoolean("pre"))
            {
                //TODO PREDICATE
                //    IObjectDataInput input = reader.GetRawDataInput();
                //    predicate = input.ReadObject<>();
                //    if (hasKey)
                //    {
                //        key = input.ReadObject();
                //    }
            }
            else
            {
                if (hasKey)
                {
                    IObjectDataInput input = reader.GetRawDataInput();
                    key = input.ReadObject<Data>();
                }
            }
        }
    }
}