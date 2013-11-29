using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Topic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Serialization.Hook
{
    public class TopicPortableHook : IPortableHook
    {
        public const int Publish = 1;
        public const int AddListener = 2;
        public const int PortableMessage = 4;
        public static readonly int FId = FactoryIdHelper.GetFactoryId(FactoryIdHelper.TopicPortableFactory, -18);

        public virtual int GetFactoryId()
        {
            return FId;
        }

        public virtual IPortableFactory CreateFactory()
        {
            var constructors = new Func<int, IPortable>[PortableMessage + 1];
            constructors[Publish] = arg => new PublishRequest();
            constructors[AddListener] = arg => new AddMessageListenerRequest();
            constructors[PortableMessage] = arg => new PortableMessage();
            return new ArrayPortableFactory(constructors);
        }

        public virtual ICollection<IClassDefinition> GetBuiltinDefinitions()
        {
            return null;
        }
    }
}