using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;


namespace Hazelcast.Util
{
    //[System.Serializable]
    //public class QueryResultSet : ICollection, IIdentifiedDataSerializable
    //{
    //    [System.NonSerialized]
    //    private readonly ISerializationService serializationService;

    //    private readonly ConcurrentBag<QueryResultEntry> entries = new ConcurrentBag<QueryResultEntry>();

    //    private IterationType iterationType;

    //    private bool data;
    //    private object _syncRoot;
    //    private bool _isSynchronized;

    //    public QueryResultSet()
    //    {
    //        serializationService = null;
    //    }

    //    public QueryResultSet(ISerializationService serializationService, IterationType iterationType, bool data)
    //    {
    //        this.serializationService = serializationService;
    //        this.data = data;
    //        this.iterationType = iterationType;
    //    }

    //    public virtual bool Add(QueryResultEntry entry)
    //    {
    //        return entries.Add(entry);
    //    }

    //    public override IEnumerator GetEnumerator()
    //    {
    //        return new QueryResultSet.QueryResultIterator(this);
    //    }

    //    private class QueryResultIterator : IEnumerator
    //    {
    //        internal readonly IEnumerator<QueryResultEntry> iter = this._enclosing.entries.GetEnumerator();

    //        public bool HasNext()
    //        {
    //            return this.iter.HasNext();
    //        }

    //        public override object Next()
    //        {
    //            QueryResultEntry entry = this.iter.Next();
    //            if (this._enclosing.iterationType == IterationType.Value)
    //            {
    //                Data valueData = entry.GetValueData();
    //                return (this._enclosing.data) ? valueData : this._enclosing.serializationService.ToObject(valueData);
    //            }
    //            else
    //            {
    //                if (this._enclosing.iterationType == IterationType.Key)
    //                {
    //                    Data keyData = entry.GetKeyData();
    //                    return (this._enclosing.data) ? keyData : this._enclosing.serializationService.ToObject(keyData);
    //                }
    //                else
    //                {
    //                    Data keyData = entry.GetKeyData();
    //                    Data valueData = entry.GetValueData();
    //                    var keyValuePair = new KeyValuePair<object, object>(this._enclosing.serializationService.ToObject(keyData), this._enclosing.serializationService.ToObject(valueData));

    //                    var valuePair = new KeyValuePair<Data, Data>(keyData, valueData);

    //                    return (this._enclosing.data) ? valuePair : keyValuePair;
    //                }
    //            }
    //        }

    //        public override void Remove()
    //        {
    //            throw new NotSupportedException();
    //        }

    //        internal QueryResultIterator(QueryResultSet _enclosing)
    //        {
    //            this._enclosing = _enclosing;
    //        }

    //        private readonly QueryResultSet _enclosing;
    //    }

    //    public void CopyTo(Array array, int index)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override int Count
    //    {
    //        get
    //        {
    //            return entries.Count;
    //        }
    //    }

    //    public object SyncRoot
    //    {
    //        get { return _syncRoot; }
    //    }

    //    public bool IsSynchronized
    //    {
    //        get { return _isSynchronized; }
    //    }

    //    public virtual int GetFactoryId()
    //    {
    //        return MapDataSerializerHook.FId;
    //    }

    //    public virtual int GetId()
    //    {
    //        return MapDataSerializerHook.QueryResultSet;
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public virtual void WriteData(IObjectDataOutput output)
    //    {
    //        output.WriteBoolean(data);
    //        output.WriteUTF(iterationType.ToString());
    //        output.WriteInt(entries.Count);
    //        foreach (QueryResultEntry queryResultEntry in entries)
    //        {
    //            output.WriteObject(queryResultEntry);
    //        }
    //    }

    //    /// <exception cref="System.IO.IOException"></exception>
    //    public virtual void ReadData(IObjectDataInput input)
    //    {
    //        data = input.ReadBoolean();
    //        iterationType = IterationType.ValueOf(input.ReadUTF());
    //        int size = input.ReadInt();
    //        for (int i = 0; i < size; i++)
    //        {
    //            entries.Add((QueryResultEntry)input.ReadObject());
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder("QueryResultSet{");
    //        sb.Append("entries=").Append(entries);
    //        sb.Append(", iterationType=").Append(iterationType);
    //        sb.Append(", data=").Append(data);
    //        sb.Append('}');
    //        return sb.ToString();
    //    }
    //}

    [System.Serializable]
    internal class QueryResultSet : ICollection, IIdentifiedDataSerializable
    {

        [System.NonSerialized]
        internal readonly ISerializationService serializationService;
        internal readonly ConcurrentBag<QueryResultEntry> entries = new ConcurrentBag<QueryResultEntry>();
        internal IterationType iterationType;
        internal bool data;
        private object _syncRoot;
        private bool _isSynchronized;

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
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                return entries.Count;
            }
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
            foreach (QueryResultEntry queryResultEntry in entries)
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
                entries.Add(input.ReadObject<QueryResultEntry>());
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
            StringBuilder sb = new StringBuilder("QueryResultSet{");
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

        private readonly IEnumerator<QueryResultEntry> _enumerator;
        private object _Current;

        internal _QueryResultIterator(QueryResultSet enclosing)
        {
            this._enclosing = enclosing;
            this._enumerator = enclosing.entries.GetEnumerator();
        }

        public bool MoveNext()
        {
            if (_enumerator.MoveNext())
            {
                QueryResultEntry entry = _enumerator.Current;
                if (this._enclosing.iterationType == IterationType.Value)
                {
                    Data valueData = entry.GetValueData();
                    _Current = (this._enclosing.data) ? valueData : this._enclosing.serializationService.ToObject(valueData);
                    return true;
                }
                if (this._enclosing.iterationType == IterationType.Key)
                {
                    Data keyData = entry.GetKeyData();
                    _Current = (this._enclosing.data) ? keyData : this._enclosing.serializationService.ToObject(keyData);
                    return true;
                }
                else
                {
                    Data keyData = entry.GetKeyData();
                    Data valueData = entry.GetValueData();
                    var keyValuePair = new KeyValuePair<object, object>(this._enclosing.serializationService.ToObject(keyData), this._enclosing.serializationService.ToObject(valueData));
                    var valuePair = new KeyValuePair<object, object>(keyData, valueData);
                    _Current = (this._enclosing.data) ? valuePair : keyValuePair;
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
