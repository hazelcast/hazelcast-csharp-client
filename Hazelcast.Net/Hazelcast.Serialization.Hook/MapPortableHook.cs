using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Map;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class MapPortableHook : IPortableHook
    {
        public const int Get = 1;
        public const int Put = 2;
        public const int PutIfAbsent = 3;
        public const int TryPut = 4;
        public const int PutTransient = 5;
        public const int Set = 6;
        public const int ContainsKey = 7;
        public const int ContainsValue = 8;
        public const int Remove = 9;
        public const int RemoveIfSame = 10;
        public const int Delete = 11;
        public const int Flush = 12;
        public const int GetAll = 13;
        public const int TryRemove = 14;
        public const int Replace = 15;
        public const int ReplaceIfSame = 16;
        public const int Lock = 17;
        public const int IsLocked = 18;
        public const int Unlock = 20;
        public const int Evict = 21;
        public const int AddInterceptor = 23;
        public const int RemoveInterceptor = 24;
        public const int AddEntryListener = 25;
        public const int GetEntryView = 27;
        public const int AddIndex = 28;
        public const int KeySet = 29;
        public const int Values = 30;
        public const int EntrySet = 31;
        public const int Size = 33;
        public const int Query = 34;
        public const int SqlQuery = 35;
        public const int Clear = 36;
        public const int GetLocalMapStats = 37;
        public const int ExecuteOnKey = 38;
        public const int ExecuteOnAllKeys = 39;
        public const int PutAll = 40;
        public const int TxnRequest = 41;
        public const int TxnRequestWithSqlQuery = 42;

        public const int ExecuteWithPredicate = 43;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.MapPortableFactory, -10);

        //import com.hazelcast.map.client.MapExecuteOnAllKeysRequest;
        //import com.hazelcast.map.client.MapExecuteOnKeyRequest;
        //import com.hazelcast.map.client.MapExecuteWithPredicateRequest;
        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[ExecuteWithPredicate + 1];
            constructors[Get] = arg => new MapGetRequest(); 
            constructors[Put] = arg => new MapPutRequest(); 
            constructors[PutIfAbsent] = arg => new MapPutIfAbsentRequest(); 
            constructors[TryPut] = arg => new MapTryPutRequest(); 
            constructors[PutTransient] = arg => new MapPutTransientRequest(); 
            constructors[Set] = arg => new MapSetRequest(); 
            constructors[ContainsKey] = arg => new MapContainsKeyRequest(); 
            constructors[ContainsValue] = arg => new MapContainsValueRequest(); 
            constructors[Remove] = arg => new MapRemoveRequest(); 
            constructors[RemoveIfSame] = arg => new MapRemoveIfSameRequest(); 
            constructors[Delete] = arg => new MapDeleteRequest(); 
            constructors[Flush] = arg => new MapFlushRequest(); 
            constructors[GetAll] = arg => new MapGetAllRequest(); 
            constructors[TryRemove] = arg => new MapTryRemoveRequest(); 
            constructors[Replace] = arg => new MapReplaceRequest(); 
            constructors[ReplaceIfSame] = arg => new MapReplaceIfSameRequest(); 
            constructors[Lock] = arg => new MapLockRequest(); 
            constructors[IsLocked] = arg => new MapIsLockedRequest(); 
            constructors[Unlock] = arg => new MapUnlockRequest(); 
            constructors[Evict] = arg => new MapEvictRequest(); 
            constructors[AddInterceptor] = arg => new MapAddInterceptorRequest(); 
            constructors[RemoveInterceptor] = arg => new MapRemoveInterceptorRequest(); 
            constructors[AddEntryListener] = arg => new MapAddEntryListenerRequest(); 
            constructors[GetEntryView] = arg => new MapGetEntryViewRequest(); 
            constructors[AddIndex] = arg => new MapAddIndexRequest(); 
            constructors[KeySet] = arg => new MapKeySetRequest(); 
            constructors[Values] = arg => new MapValuesRequest(); 
            constructors[EntrySet] = arg => new MapEntrySetRequest(); 
            constructors[Size] = arg => new MapSizeRequest(); 
            constructors[Clear] = arg => new MapClearRequest(); 
            constructors[Query] = arg => new MapQueryRequest<object,object>(); 
            constructors[SqlQuery] = arg => new MapSQLQueryRequest();
            constructors[ExecuteOnKey] = delegate(int i) { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); }; 
            //arg => new MapExecuteOnKeyRequest(); 
            constructors[ExecuteOnAllKeys] = delegate(int i) { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); }; 
            //arg => new MapExecuteOnAllKeysRequest(); 
            constructors[PutAll] = arg => new MapPutAllRequest();
            constructors[TxnRequest] = arg => new TxnMapRequest<object, object>(); 
            constructors[TxnRequestWithSqlQuery] = arg => new TxnMapWithSQLQueryRequest();
            constructors[ExecuteWithPredicate] = delegate(int i) { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); }; 
            //arg => new MapExecuteWithPredicateRequest(); 

            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}