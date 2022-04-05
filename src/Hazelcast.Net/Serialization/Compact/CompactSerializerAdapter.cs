// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Reflection;
using Hazelcast.Exceptions;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Exposes a typed <see cref="ICompactSerializer{T}"/> as a non-generic serializer working with objects.
    /// </summary>
    internal class CompactSerializerAdapter
    {
        private static MethodInfo? _create;
        private readonly Func<ICompactReader, object> _read;
        private readonly Action<ICompactWriter, object> _write;

        private CompactSerializerAdapter(ICompactSerializer serializer, Func<ICompactReader, object> read, Action<ICompactWriter, object> write)
        {
            Serializer = serializer;
            _read = read;
            _write = write;
        }

        /// <summary>
        /// Gets the wrapped serializer.
        /// </summary>
        public ICompactSerializer Serializer { get; }

        // provides the generic create method
        private static MethodInfo CreateMethod
        {
            get
            {
                if (_create != null) return _create;
                _create = typeof(CompactSerializerAdapter).GetMethod(nameof(Create_), BindingFlags.NonPublic | BindingFlags.Static);
                if (_create == null) throw new HazelcastException(); // internal error
                return _create;
            }
        }
        
        // this method exists only so that it's easy to GetMethod(...) in CreateMethod because Create is overloaded
        private static CompactSerializerAdapter Create_<T>(ICompactSerializer<T> serializer) where T : notnull
            => Create(serializer);

        /// <summary>
        /// Creates a <see cref="CompactSerializerAdapter"/> for an <see cref="ICompactSerializer{T}"/>.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <typeparam name="T">The serialized type.</typeparam>
        /// <returns>A <see cref="CompactSerializerAdapter"/> for the <paramref name="serializer"/>.</returns>
        public static CompactSerializerAdapter Create<T>(ICompactSerializer<T> serializer) where T : notnull
            => new CompactSerializerAdapter(
                    serializer,
                    // ReSharper disable once HeapView.PossibleBoxingAllocation - accepted, T can be struct
                    reader => serializer.Read(reader),
                    (writer, obj) => serializer.Write(writer, (T)obj)
                );

        /// <summary>
        /// Creates a <see cref="CompactSerializerAdapter"/> for an <see cref="ICompactSerializer"/>.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A <see cref="CompactSerializerAdapter"/> for the <paramref name="serializer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serializer"/> is <c>null</c>.</exception>
        public static CompactSerializerAdapter Create(ICompactSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            // note: this is going to happen once per serializedType, no point caching the created method
            
            var serializedType = serializer.GetSerializedType();
            var obj = CreateMethod.MakeGenericMethod(serializedType).Invoke(null, new object[] { serializer });
            if (obj is CompactSerializerAdapter wrapper) return wrapper;
            throw new HazelcastException(ExceptionMessages.InternalError);
        }
        
        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="reader">The compact reader.</param>
        /// <returns>The object.</returns>
        public object Read(ICompactReader reader) => _read(reader);

        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="obj">The object.</param>
        public void Write(ICompactWriter writer, object obj) => _write(writer, obj);
    }
}
