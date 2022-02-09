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

namespace Hazelcast.Serialization.Compact
{
    internal class CompactSerializerWrapper
    {
        private readonly Func<ICompactReader, object?> _read;
        private readonly Action<ICompactWriter, object?> _write;

        protected CompactSerializerWrapper(Func<ICompactReader, object?> read, Action<ICompactWriter, object?> write)
        {
            _read = read;
            _write = write;
        }

        public static CompactSerializerWrapper Create<T>(ICompactSerializer<T> serializer)
            => new CompactSerializerWrapper(
                reader => serializer.Read(reader),
                (writer, obj) => serializer.Write(writer, (T)obj)
            );

        // FIXME - dead code
        /*
        public static CompactSerializerWrapper Create(ICompactable compactable)
        {
            var typeOfCompactable = compactable.GetType();
            var interfaces = typeOfCompactable.GetInterfaces();

            foreach (var i in interfaces)
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ICompactable<>))
                {
                    var t = i.GetGenericArguments()[0];
                    var m = typeof (CompactSerializerWrapper).GetMethod("CreateFromCompactable", BindingFlags.NonPublic | BindingFlags.Static);
                    if (m == null) throw new InvalidOperationException();
                    var mm = m.MakeGenericMethod(t);
                    return (CompactSerializerWrapper) mm.Invoke(null, new [] { compactable });
                }
            }

            throw new ArgumentException();
        }
        */

        public static CompactSerializerWrapper Create(Type type, Type? serializedType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var serializer = Activator.CreateInstance(type);
            var typeOfSerializer = serializer.GetType();
            var interfaces = typeOfSerializer.GetInterfaces();

            foreach (var i in interfaces)
            {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICompactSerializer<>))
                {
                    // FIXME cache this, etc.
                    var t = i.GetGenericArguments()[0];
                    if (t != serializedType)
                        throw new InvalidOperationException(""); // FIXME - isAssignable + exception
                    // FIXME method detection horror
                    var m = typeof(CompactSerializerWrapper).GetMethod("CreateFromSerializer", BindingFlags.NonPublic | BindingFlags.Static);
                    if (m == null) throw new InvalidOperationException();
                    var mm = m.MakeGenericMethod(t);
                    return (CompactSerializerWrapper)mm.Invoke(null, new[] { serializer });
                }
            }

            throw new ArgumentException();
        }

        // FIXME - dead code
        /*
        private static CompactSerializerWrapper CreateFromCompactable<T>(ICompactable<T> compactable)
        {
            var serializer = compactable.GetSerializer();

            return new CompactSerializerWrapper(
                reader => serializer.Read(reader),
                (writer, obj) => serializer.Write(writer, (T)obj)
            );
        }
        */

        // FIXME not needed if we stop being stupid getting methods
        private static CompactSerializerWrapper CreateFromSerializer<T>(ICompactSerializer<T> serializer)
            => Create(serializer);

        public object? Read(ICompactReader reader) => _read(reader);

        public void Write(ICompactWriter writer, object obj) => _write(writer, obj);
    }
}
