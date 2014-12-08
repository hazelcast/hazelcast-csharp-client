using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    internal class QueuePortableHook : IPortableHook
    {
        public const int Offer = 1;
        public const int Size = 2;
        public const int Remove = 3;
        public const int Poll = 4;
        public const int Peek = 5;
        public const int Iterator = 6;
        public const int Drain = 7;
        public const int Contains = 8;
        public const int CompareAndRemove = 9;
        public const int Clear = 10;
        public const int AddAll = 11;
        public const int AddListener = 12;
        public const int RemainingCapacity = 13;
        public const int TxnOffer = 14;
        public const int TxnPoll = 15;
        public const int TxnSize = 16;
        public const int TxnPeek = 17;
        public const int RemoveListener = 18;
        public const int IsEmpty = 19;

        public const int TotalSize = IsEmpty + 1;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.QueuePortableFactory, -11);

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