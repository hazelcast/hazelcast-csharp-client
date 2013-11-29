using System;
using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public sealed class SpiDataSerializerHook : DataSerializerHook
    {
        internal const int Response = 0;
        internal const int Backup = 1;
        internal const int BackupResponse = 2;
        internal const int PartitionIterator = 3;
        internal const int PartitionResponse = 4;
        internal const int ParallelOperationFactory = 5;
        internal const int EventPacket = 6;
        public const int Collection = 7;
        private const int Len = 10;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.SpiDsFactory, -1);

        //import com.hazelcast.spi.impl.EventServiceImpl.EventPacket;
        //import com.hazelcast.spi.impl.PartitionIteratingOperation.PartitionResponse;
        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<int, IIdentifiedDataSerializable>[Len];
            constructors[Response] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[Backup] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new Backup();
            constructors[BackupResponse] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new BackupResponse();
            constructors[EventPacket] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new EventPacket();
            constructors[PartitionIterator] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            constructors[PartitionResponse] = delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new PartitionResponse();
            constructors[ParallelOperationFactory] =
                delegate { throw new NotSupportedException("NOT IMPLEMENTED ON CLIENT"); };
            //                return new BinaryOperationFactory();
            constructors[Collection] = delegate { return new SerializableCollection(); };

            return new ArrayDataSerializableFactory(constructors);
        }


        public int GetFactoryId()
        {
            return FId;
        }
    }
}