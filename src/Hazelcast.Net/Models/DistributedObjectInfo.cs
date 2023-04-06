// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Hazelcast.Models
{
    /// <summary>
    /// Describes a distributed object.
    /// </summary>
    public class DistributedObjectInfo : IEquatable<DistributedObjectInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedObjectInfo"/> class.
        /// </summary>
        /// <param name="serviceName">The object service name.</param>
        /// <param name="name">The object name.</param>
        internal DistributedObjectInfo(string serviceName, string name)
        {
            Name = name;
            ServiceName = serviceName;
        }

        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the service name of the object.
        /// </summary>
        public string ServiceName { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => Equals(obj as DistributedObjectInfo);

        /// <inheritdoc />
        public bool Equals(DistributedObjectInfo other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                Name == other.Name &&
                ServiceName == other.ServiceName;
        }

        /// <summary>
        /// Determines whether two <see cref="DistributedObjectInfo"/> instances are equal.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns><c>true</c> if the instances are equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(DistributedObjectInfo left, DistributedObjectInfo right)
            // ReSharper disable once MergeConditionalExpression
            => left is null ? right is null : left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="DistributedObjectInfo"/> instances are different.
        /// </summary>
        /// <param name="left">The left instance.</param>
        /// <param name="right">The right instance.</param>
        /// <returns><c>true</c> if the instances are different; otherwise <c>false</c>.</returns>
        public static bool operator !=(DistributedObjectInfo left, DistributedObjectInfo right)
            => !(left == right);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(ServiceName, Name);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"ServiceName = '{ServiceName}', Name = '{Name}'";
        }
    }
}
