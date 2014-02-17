using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Map;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class MapPortableHook : IPortableHook
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
        //public const int SqlQuery = 35;
        public const int Clear = 36;
        public const int GetLocalMapStats = 37;
        public const int ExecuteOnKey = 38;
        public const int ExecuteOnAllKeys = 39;
        public const int PutAll = 40;
        public const int TxnRequest = 41;
        public const int TxnRequestWithSqlQuery = 42;
        public const int ExecuteWithPredicate = 43;
        public const int RemoveEntryListener = 44;
        public const int ExecuteOnKeys = 45;

        public const int TotalSize = ExecuteOnKeys + 1;

        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.MapPortableFactory, -10);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            return new ArrayPortableFactory();
        }


        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}