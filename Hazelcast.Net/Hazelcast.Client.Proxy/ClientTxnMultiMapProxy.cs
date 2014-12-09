using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnMultiMapProxy<K, V> : ClientTxnProxy, ITransactionalMultiMap<K, V>
    {
        public ClientTxnMultiMapProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
        {
        }

        /// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        public virtual bool Put(K key, V value)
        {
            var request = new TxnMultiMapPutRequest(GetName(), ToData(key), ToData(value));
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual ICollection<V> Get(K key)
        {
            var request = new TxnMultiMapGetRequest(GetName(), ToData(key));
            var portableCollection = Invoke<PortableCollection>(request);
            ICollection<IData> collection = portableCollection.GetCollection();
            ICollection<V> coll;
            if (collection is IList)
            {
                coll = new List<V>(collection.Count);
            }
            else
            {
                coll = new HashSet<V>();
            }
            foreach (IData data in collection)
            {
                coll.Add( ToObject<V>(data));
            }
            return coll;
        }

        public virtual bool Remove(object key, object value)
        {
            var request = new TxnMultiMapRemoveRequest(GetName(), ToData(key), ToData(value));
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual ICollection<V> Remove(object key)
        {
            var request = new TxnMultiMapRemoveAllRequest(GetName(), ToData(key));
            var portableCollection = Invoke<PortableCollection>(request);
            ICollection<IData> collection = portableCollection.GetCollection();
            ICollection<V> coll;
            if (collection is IList)
            {
                coll = new List<V>(collection.Count);
            }
            else
            {
                coll = new HashSet<V>();
            }
            foreach (IData data in collection)
            {
                coll.Add( ToObject<V>(data));
            }
            return coll;
        }

        public virtual int ValueCount(K key)
        {
            var request = new TxnMultiMapValueCountRequest(GetName(), ToData(key));
            var result = Invoke<int>(request);
            return result;
        }

        public virtual int Size()
        {
            var request = new TxnMultiMapSizeRequest(GetName());
            var result = Invoke<int>(request);
            return result;
        }

        public override string GetName()
        {
            return (string) GetId();
        }

        public override string GetServiceName()
        {
            return ServiceNames.MultiMap;
        }

        internal override void OnDestroy()
        {
        }
    }
}