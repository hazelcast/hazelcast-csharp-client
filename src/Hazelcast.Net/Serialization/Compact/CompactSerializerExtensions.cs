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

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Provides extension methods for working with compact serializers.
    /// </summary>
    internal static class CompactSerializerExtensions
    {
        private static readonly Type TypeOfICompactSerializerOfTSerialized = typeof(ICompactSerializer<>);

        /// <summary>
        /// Gets the type serialized by an <see cref="ICompactSerializer"/>.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>The type serialized by the serializer.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is null.</exception>
        /// <exception cref="ArgumentException">The serializer is not an <see cref="ICompactSerializer{TSerialized}"/>.</exception>
        public static Type GetSerializedType(this ICompactSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (!serializer.IsICompactSerializerOfTSerialized(out var serializedType))
                throw new ArgumentException("Serializer does not implement ICompactSerializer<TSerialized>.");
            return serializedType;
        }

        /// <summary>
        /// Verifies that an <see cref="ICompactSerializer"/> is an <see cref="ICompactSerializer{TSerialized}"/> and provides the serialized type.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="serializedType">The serialized type.</param>
        /// <returns><c>true</c> if the <paramref name="serializer"/> is an <see cref="ICompactSerializer{TSerialized}"/>, and
        /// then <paramref name="serializedType"/> contains the serialized type; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is null.</exception>
        public static bool IsICompactSerializerOfTSerialized(this ICompactSerializer serializer, [NotNullWhen(true)] out Type? serializedType)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return IsICompactSerializerOfTSerialized(serializer.GetType(), out serializedType);
        }

        /// <summary>
        /// Verifies that an <see cref="ICompactSerializer"/> is an <see cref="ICompactSerializer{TSerialized}"/> and provides the serialized type.
        /// </summary>
        /// <param name="serializerType">The serializer type.</param>
        /// <param name="serializedType">The serialized type.</param>
        /// <returns><c>true</c> if the <paramref name="serializerType"/> is an <see cref="ICompactSerializer{TSerialized}"/>, and
        /// then <paramref name="serializedType"/> contains the serialized type; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializerType"/> is null.</exception>
        public static bool IsICompactSerializerOfTSerialized(this Type serializerType, [NotNullWhen(true)] out Type? serializedType)
        {
            if (serializerType == null) throw new ArgumentNullException(nameof(serializerType));

            var i = serializerType
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == TypeOfICompactSerializerOfTSerialized);

            serializedType = i?.GetGenericArguments()[0];
            return i != null;
        }
    }
}
