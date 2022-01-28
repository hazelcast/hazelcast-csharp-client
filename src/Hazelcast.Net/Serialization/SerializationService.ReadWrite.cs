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

using System;
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;

namespace Hazelcast.Serialization
{
    internal partial class SerializationService
    {
        /// <summary>
        /// Serializes an object to an <see cref="IData"/> blob.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The <see cref="IData"/> blob.</returns>
        /// <exception cref="SerializationException">Failed to serialize the object (see inner exception).</exception>
        public IData ToData(object obj)
            => ToData(obj, _globalPartitioningStrategy);

        /// <summary>
        /// Serializes an object to an <see cref="IData"/> blob.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="strategy">A partitioning strategy.</param>
        /// <returns>The <see cref="IData"/> blob.</returns>
        /// <exception cref="SerializationException">Failed to serialize the object (see inner exception).</exception>
        public IData ToData(object obj, IPartitioningStrategy strategy)
        {
            if (obj is null) return null;
            if (obj is IData data) return data;

            var output = GetDataOutput();

            try
            {
                var partitionHash = CalculatePartitionHash(obj, strategy);
                output.WriteIntBigEndian(partitionHash); // partition hash is always big-endian
                WriteObject(output, obj, true);
                return new HeapData(output.ToByteArray());
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
            finally
            {
                ReturnDataOutput(output);
            }
        }

        /// <summary>
        /// Deserializes an <see cref="IData"/> blob to an object.
        /// </summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="dataObj">The <see cref="IData"/> blob.</param>
        /// <returns>The object.</returns>
        /// <exception cref="SerializationException">Failed to deserialize the object (see inner exception).</exception>
        /// <exception cref="InvalidCastException">Failed to case deserialized object to type <typeparamref name="T"/>.</exception>
        public T ToObject<T>(object dataObj)
        {
            var obj = ToObject(dataObj);
            return CastObject<T>(obj, false);
        }

        /// <summary>
        /// Deserializes an <see cref="IData"/> blob to an object.
        /// </summary>
        /// <param name="dataObj">The <see cref="IData"/> blob.</param>
        /// <returns>The object.</returns>
        /// <exception cref="SerializationException">Failed to deserialize the object (see inner exception).</exception>
        public object ToObject(object dataObj)
        {
            if (!(dataObj is IData data)) return dataObj;

            var input = GetDataInput(data);
            try
            {
                var typeId = data.TypeId;
                var serializer = LookupSerializer(typeId);
                return serializer.Read(input);
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
            finally
            {
                ReturnDataInput(input);
            }
        }

        /// <summary>
        /// Writes an object to an <see cref="ObjectDataOutput"/>.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="obj">The object.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="output"/> is <c>null</c>.</exception>
        /// <exception cref="SerializationException">Failed to serialize the object (see inner exception).</exception>
        public void WriteObject(ObjectDataOutput output, object obj)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (obj is IData) throw new SerializationException("Cannot write IData. Use WriteData instead.");

            WriteObject(output, obj, false);
        }

        private void WriteObject(ObjectDataOutput output, object obj, bool isRootObject)
        {
            // args checks performed by callers

            try
            {
                var serializer = LookupSerializer(obj, false); // FIXME - withSchema?!

                // root object (from ToData) type-id is always big-endian, whereas
                // nested objects type-id uses whatever is the default endianness
                if (isRootObject) output.WriteIntBigEndian(serializer.TypeId);
                else output.WriteInt(serializer.TypeId);

                serializer.Write(output, obj);
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
        }

        /// <summary>
        /// Reads an object from an <see cref="ObjectDataInput"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="input">The input.</param>
        /// <returns>The object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="input"/> is <c>null</c>.</exception>
        /// <exception cref="SerializationException">Failed to deserialize the object (see inner exception).</exception>
        /// <exception cref="InvalidCastException">Failed to case deserialized object to type <typeparamref name="T"/>.</exception>
        public T ReadObject<T>(ObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            try
            {
                // root object type-id is always big endian, and this is managed in HeapData,
                // here we are only reading nested objects and their type-id uses whatever is
                // the default endianness.
                var typeId = input.ReadInt();
                var serializer = LookupSerializer(typeId);

                var obj = serializer.Read(input);
                return CastObject<T>(obj, true);
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
        }

        private static T CastObject<T>(object obj, bool enforceNullable)
        {
            // when getting a IHMap<int, int> value for a non-existing key, the cluster will return
            // a null value, and this is not an error, so ToObject<T> has to deserialize it somehow,
            // not throw, but fall back to default -> enforceNullable is false.
            //
            // OTOH ReadObject<T> (not used by ToObject) should not get a null value for anything
            // that is not nullable, and therefore should throw -> enforceNullable is true.

            return obj switch
            {
                T t => t,

                null when !enforceNullable || typeof(T).IsNullable() => default,
                null => throw new SerializationException($"Cannot cast deserialized null value to value type {typeof(T)}."),

                _ => throw new InvalidCastException($"Cannot cast deserialized object of type {obj.GetType()} to type {typeof(T)}.")
            };
        }
    }
}
