using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Util
{
 
    [Serializable]
    internal class QueryResultSet : IdentifiedDataSerializable,ICollection, IIdentifiedDataSerializable
    {
        internal readonly ConcurrentBag<IQueryResultEntry> entries = new ConcurrentBag<IQueryResultEntry>();
        
        [NonSerialized] 
        internal readonly ISerializationService serializationService;
        
        private bool _isSynchronized;
        private object _syncRoot;
        internal bool data;
        internal IterationType iterationType;

        public QueryResultSet()
        {
            serializationService = null;
        }

        public IEnumerator GetEnumerator()
        {
            return new _QueryResultIterator(this);
        }

        public void CopyTo(Array array, int index)
        {
            int i = index;
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {

                array.SetValue(enumerator.Current,i);
            }
        }

        public int Count
        {
            get { return entries.Count; }
        }

        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        public bool IsSynchronized
        {
            get { return _isSynchronized; }
        }

        public virtual void WriteData(IObjectDataOutput output)
        {
            output.WriteBoolean(data);
            output.WriteUTF(iterationType.ToString());
            output.WriteInt(entries.Count);
            foreach (IQueryResultEntry queryResultEntry in entries)
            {
                output.WriteObject(queryResultEntry);
            }
        }


        public virtual void ReadData(IObjectDataInput input)
        {
            data = input.ReadBoolean();
            Enum.TryParse(input.ReadUTF(), true, out iterationType);
            int size = input.ReadInt();
            for (int i = 0; i < size; i++)
            {
                entries.Add(input.ReadObject<IQueryResultEntry>());
            }
        }

        public int GetFactoryId()
        {
            return MapDataSerializerHook.FId;
        }

        public int GetId()
        {
            return MapDataSerializerHook.QueryResultSet;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("QueryResultSet{");
            sb.Append("entries=").Append(entries);
            sb.Append(", iterationType=").Append(iterationType);
            sb.Append(", data=").Append(data);
            sb.Append('}');
            return sb.ToString();
        }
    }

    internal class _QueryResultIterator : IEnumerator
    {
        private readonly QueryResultSet _enclosing;

        private readonly IEnumerator<IQueryResultEntry> _enumerator;
        private object _Current;

        internal _QueryResultIterator(QueryResultSet enclosing)
        {
            _enclosing = enclosing;
            _enumerator = enclosing.entries.GetEnumerator();
        }

        public bool MoveNext()
        {
            if (_enumerator.MoveNext())
            {
                IQueryResultEntry entry = _enumerator.Current;
                if (_enclosing.iterationType == IterationType.Value)
                {
                    IData valueData = entry.GetValueData();
                    _Current = (_enclosing.data) ? valueData : _enclosing.serializationService.ToObject<object>(valueData);
                    return true;
                }
                if (_enclosing.iterationType == IterationType.Key)
                {
                    IData keyData = entry.GetKeyData();
                    _Current = (_enclosing.data) ? keyData : _enclosing.serializationService.ToObject<object>(keyData);
                    return true;
                }
                else
                {
                    IData keyData = entry.GetKeyData();
                    IData valueData = entry.GetValueData();
                    var keyValuePair =
                        new KeyValuePair<object, object>(_enclosing.serializationService.ToObject<object>(keyData),
                            _enclosing.serializationService.ToObject<object>(valueData));
                    var valuePair = new KeyValuePair<object, object>(keyData, valueData);
                    _Current = (_enclosing.data) ? valuePair : keyValuePair;
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public object Current
        {
            get { return _Current; }
        }
    }
}