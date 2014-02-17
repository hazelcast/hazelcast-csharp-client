using System;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class PortableEntrySetResponse : IPortable
    {
        internal ICollection<KeyValuePair<Data, Data>> entrySet = null;

        public PortableEntrySetResponse()
        {
        }

        public PortableEntrySetResponse(ICollection<KeyValuePair<Data, Data>> entrySet)
        {
            this.entrySet = entrySet;
        }

        public virtual int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        public virtual int GetClassId()
        {
            return MultiMapPortableHook.EntrySetResponse;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            throw new NotSupportedException();

        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            int size = reader.ReadInt("s");
            IObjectDataInput input = reader.GetRawDataInput();
            entrySet = new HashSet<KeyValuePair<Data, Data>>();
            for (int i = 0; i < size; i++)
            {
                var key = new Data();
                var value = new Data();
                key.ReadData(input);
                value.ReadData(input);
                entrySet.Add(new KeyValuePair<Data, Data>(key, value));
            }
        }

        public virtual ICollection<KeyValuePair<Data, Data>> GetEntrySet()
        {
            return entrySet;
        }
    }
}