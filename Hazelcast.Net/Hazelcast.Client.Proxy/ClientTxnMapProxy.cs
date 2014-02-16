using System.Collections.Generic;
using Hazelcast.Client.Request.Map;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Proxy
{
    public class ClientTxnMapProxy<K, V> : ClientTxnProxy, ITransactionalMap<K, V>
    {
        public ClientTxnMapProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
        {
        }

        public virtual bool ContainsKey(object key)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.ContainsKey,
                ToData(key));
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual V Get(object key)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Get, ToData(key));
            return Invoke<V>(request);
        }

        public virtual V GetForUpdate(object key)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.GetForUpdate,
                ToData(key));
            return Invoke<V>(request);
        }

        public virtual int Size()
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Size);
            var result = Invoke<int>(request);
            return result;
        }

        public virtual bool IsEmpty()
        {
            return Size() == 0;
        }

        public virtual V Put(K key, V value)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Put, ToData(key),
                ToData(value));
            return Invoke<V>(request);
        }

        public V Put(K key, V value, long ttl, TimeUnit timeunit)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.PuttWithTTL, ToData(key),
                ToData(value),ttl,timeunit);
            return Invoke<V>(request);
        }

        public virtual void Set(K key, V value)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Set, ToData(key),
                ToData(value));
            Invoke<V>(request);
        }

        public virtual V PutIfAbsent(K key, V value)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.PutIfAbsent,
                ToData(key), ToData(value));
            return Invoke<V>(request);
        }

        public virtual V Replace(K key, V value)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Replace,
                ToData(key), ToData(value));
            return Invoke<V>(request);
        }

        public virtual bool Replace(K key, V oldValue, V newValue)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.ReplaceIfSame,
                ToData(key), ToData(oldValue), ToData(newValue));
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual V Remove(object key)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Remove, ToData(key));
            return Invoke<V>(request);
        }

        public virtual void Delete(object key)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Delete, ToData(key));
            Invoke<object>(request);
        }

        public virtual bool Remove(object key, object value)
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.RemoveIfSame,
                ToData(key), ToData(value));
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual ICollection<K> KeySet()
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Keyset);
            var result = Invoke<MapKeySet>(request);
            ICollection<Data> dataKeySet = result.GetKeySet();
            var keySet = new HashSet<K>();
            foreach (Data data in dataKeySet)
            {
                keySet.Add( ToObject<K>(data));
            }
            return keySet;
        }

        //public virtual ICollection<K> KeySet(IPredicate predicate)
        //{
        //    if (predicate == null)
        //    {
        //        throw new ArgumentNullException("IPredicate should not be null!");
        //    }
        //    var request = new TxnMapRequest<K,V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.KeysetByPredicate, predicate);
        //    MapKeySet result = Invoke(request);
        //    ICollection<Data> dataKeySet = result.GetKeySet();
        //    HashSet<K> keySet = new HashSet<K>(dataKeySet.Count);
        //    foreach (Data data in dataKeySet)
        //    {
        //        keySet.Add((K)ToObject(data));
        //    }
        //    return keySet;
        //}

        public virtual ICollection<V> Values()
        {
            var request = new TxnMapRequest<K, V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.Values);
            var result = Invoke<MapValueCollection>(request);
            ICollection<Data> dataValues = result.GetValues();
            var values = new HashSet<V>();
            foreach (Data value in dataValues)
            {
                values.Add( ToObject<V>(value));
            }
            return values;
        }

        //public virtual ICollection<V> Values(IPredicate predicate)
        //{
        //    if (predicate == null)
        //    {
        //        throw new ArgumentNullException("IPredicate should not be null!");
        //    }
        //    var request = new TxnMapRequest<K,V>(GetName(), AbstractTxnMapRequest.TxnMapRequestType.ValuesByPredicate, predicate);
        //    MapValueCollection result = Invoke(request);
        //    ICollection<Data> dataValues = result.GetValues();
        //    HashSet<V> values = new HashSet<V>(dataValues.Count);
        //    foreach (Data value in dataValues)
        //    {
        //        values.Add((V)ToObject(value));
        //    }
        //    return values;
        //}

        public override string GetName()
        {
            return (string) GetId();
        }

        public override string GetServiceName()
        {
            return ServiceNames.Map;
        }

        internal override void OnDestroy()
        {
        }
    }
}