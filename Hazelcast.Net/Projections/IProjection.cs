using Hazelcast.Aggregators;
using Hazelcast.Serialization;

namespace Hazelcast.Projections
{
    /// <summary>
    /// Defines a projection that can transform an object into other objects.
    /// </summary>
    /// <remarks>
    /// <para>Only 1-to-1 projections are allowed. Use an <see cref="IAggregator{TResult}"/> to perform n-to-1 or
    /// n-to-n projections.</para>
    /// <para>Projections must have a server-side counterpart.</para>
    /// </remarks>
    public interface IProjection : IIdentifiedDataSerializable
    { }
}
