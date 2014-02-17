using System;
using System.Collections.Generic;
using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;
using Hazelcast.Security;

namespace Hazelcast.Serialization.Hook
{
    internal sealed class SpiPortableHook : IPortableHook
    {
        public const int UsernamePwdCred = 1;
        public const int Collection = 2;
        public const int ItemEvent = 3;
        public const int EntryEvent = 4;
        public const int DistributedObjectEvent = 5;
        public static readonly int Id = FactoryIdHelper.GetFactoryId(FactoryIdHelper.SpiPortableFactory, -1);

        public int GetFactoryId()
        {
            return Id;
        }

        public IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[DistributedObjectEvent + 1];
            constructors[UsernamePwdCred] = arg => new UsernamePasswordCredentials();
            constructors[Collection] = arg => new PortableCollection();
            constructors[ItemEvent] = arg => new PortableItemEvent();
            constructors[EntryEvent] = arg => new PortableEntryEvent();
            constructors[DistributedObjectEvent] = arg => new PortableDistributedObjectEvent();
            return new ArrayPortableFactory(constructors);
        }

        public ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}