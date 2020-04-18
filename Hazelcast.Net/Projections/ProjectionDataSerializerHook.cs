using System;
using Hazelcast.Serialization;

namespace Hazelcast.Projections
{
    public class ProjectionDataSerializerHook : IDataSerializerHook // fixme rename+
    {
        public const int FactoryId = FactoryIds.ProjectionDsFactoryId;
        public const int SingleAttribute = 0;
        public const int MultiAttribute = 1;

        private const int Len = MultiAttribute + 1;

        /// <inheritdoc />
        public IDataSerializableFactory CreateFactory()
        {
            var constructors = new Func<IIdentifiedDataSerializable>[Len];
            constructors[SingleAttribute] = () => new SingleAttributeProjection();

            return new ArrayDataSerializableFactory(constructors);
        }

        /// <inheritdoc />
        public int GetFactoryId() => FactoryId;
    }
}
