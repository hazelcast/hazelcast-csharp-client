using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;


namespace Hazelcast.Client.Proxy
{
	public class ClientTxnMultiMapProxy<K, V> : ClientTxnProxy, ITransactionalMultiMap<K, V>
	{
		public ClientTxnMultiMapProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
		{
		}

		/// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
		public virtual bool Put(K key, V value)
		{
			TxnMultiMapPutRequest request = new TxnMultiMapPutRequest(GetName(), ToData(key), ToData(value));
			bool result = Invoke<bool>(request);
			return result;
		}

		public virtual ICollection<V> Get(K key)
		{
			TxnMultiMapGetRequest request = new TxnMultiMapGetRequest(GetName(), ToData(key));
            PortableCollection portableCollection = Invoke<PortableCollection>(request);
			ICollection<Data> collection = portableCollection.GetCollection();
			ICollection<V> coll;
			if (collection is IList)
			{
				coll = new List<V>(collection.Count);
			}
			else
			{
				coll = new HashSet<V>();
			}
			foreach (Data data in collection)
			{
				coll.Add((V)ToObject(data));
			}
			return coll;
		}

		public virtual bool Remove(object key, object value)
		{
			TxnMultiMapRemoveRequest request = new TxnMultiMapRemoveRequest(GetName(), ToData(key), ToData(value));
			bool result = Invoke<bool>(request);
			return result;
		}

		public virtual ICollection<V> Remove(object key)
		{
			TxnMultiMapRemoveRequest request = new TxnMultiMapRemoveRequest(GetName(), ToData(key));
            PortableCollection portableCollection = Invoke<PortableCollection>(request);
			ICollection<Data> collection = portableCollection.GetCollection();
			ICollection<V> coll;
			if (collection is IList)
			{
				coll = new List<V>(collection.Count);
			}
			else
			{
				coll = new HashSet<V>();
			}
			foreach (Data data in collection)
			{
				coll.Add((V)ToObject(data));
			}
			return coll;
		}

		public virtual int ValueCount(K key)
		{
			TxnMultiMapValueCountRequest request = new TxnMultiMapValueCountRequest(GetName(), ToData(key));
			int result = Invoke<int>(request);
			return result;
		}

		public virtual int Size()
		{
			TxnMultiMapSizeRequest request = new TxnMultiMapSizeRequest(GetName());
			int result = Invoke<int>(request);
			return result;
		}

		public override string GetName()
		{
			return (string)GetId();
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
