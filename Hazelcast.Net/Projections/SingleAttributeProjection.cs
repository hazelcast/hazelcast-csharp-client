using System;
using Hazelcast.Exceptions;
using Hazelcast.Serialization;

namespace Hazelcast.Projections
{
    /// <summary>
    /// Represents a simple attribute projection.
    /// </summary>
    public class SingleAttributeProjection : IProjection
    {
        private string _attributePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleAttributeProjection"/> class/.
        /// </summary>
        public SingleAttributeProjection()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleAttributeProjection"/> class/.
        /// </summary>
        /// <param name="attributPath">The attribute path.</param>
        public SingleAttributeProjection(string attributePath)
        {
            if (string.IsNullOrWhiteSpace(attributePath)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(attributePath));
            _attributePath = attributePath;
        }

        /// <inheritdoc />
        public void ReadData(IObjectDataInput input)
        {
            _attributePath = input.ReadUtf();
        }

        /// <inheritdoc />
        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUtf(_attributePath);
        }

        /// <inheritdoc />
        public int GetFactoryId()
        {
            return FactoryIds.ProjectionDsFactoryId;
        }

        /// <inheritdoc />
        public int GetId()
        {
            return ProjectionDataSerializerHook.SingleAttribute;
        }
    }
}
