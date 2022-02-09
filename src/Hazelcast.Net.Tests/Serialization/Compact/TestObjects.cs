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

using System.Threading;
using Hazelcast.Serialization.Compact;

namespace Hazelcast.Tests.Serialization.Compact
{
    internal interface IThing
    {
        string? Name { get; set; }

        int Value { get; set; }
    }

    internal class Thing : IThing
    {
        public const string TypeName = "thing";

        public static class FieldNames
        {
            public const string Name = "name";
            public const string Value = "value";
        }

        public string? Name { get; set; }

        public int Value { get; set; }
    }

    // FIXME - dead code
    /*
    internal class ThingCompactableInterface : IThing, ICompactable<ThingCompactableInterface>
    {
        public const string TypeName = "cthing1";

        public string Name { get; set; }

        public int Value { get; set; }

        ICompactSerializer<ThingCompactableInterface> ICompactable<ThingCompactableInterface>.GetSerializer() 
            => new ThingCompactSerializer<ThingCompactableInterface>();

        string ICompactable.TypeName => default;

        bool? ICompactable.PublishedSchema => default;
    }

    internal class ThingCompactableInterfaceWithTypeName : IThing, ICompactable<ThingCompactableInterfaceWithTypeName>
    {
        public const string TypeName = "cthing2";

        public string Name { get; set; }

        public int Value { get; set; }

        ICompactSerializer<ThingCompactableInterfaceWithTypeName> ICompactable<ThingCompactableInterfaceWithTypeName>.GetSerializer() 
            => new ThingCompactSerializer<ThingCompactableInterfaceWithTypeName>();

        string ICompactable.TypeName => TypeName;

        bool? ICompactable.PublishedSchema => default;
    }
    */

    // FIXME - dead code
    /*
    [CompactSerializable(typeof(ThingCompactSerializer<ThingCompactableAttribute>))]
    internal class ThingCompactableAttribute : IThing
    {
        public const string TypeName = "cthing1";

        public string Name { get; set; }

        public int Value { get; set; }
    }

    [CompactSerializable(typeof(ThingCompactSerializer<ThingCompactableAttributeWithTypeName>), TypeName = TypeName)]
    internal class ThingCompactableAttributeWithTypeName : IThing
    {
        public const string TypeName = "cthing1";

        public string Name { get; set; }

        public int Value { get; set; }
    }
    */

    internal static class ThingCompactSerializer
    {
        private static int _readCount;
        private static int _writeCount;

        public static void Reset()
        {
            _readCount = _writeCount = 0;
        }

        public static void CountRead() => Interlocked.Increment(ref _readCount);

        public static void CountWrite() => Interlocked.Increment(ref _writeCount);

        public static int ReadCount => _readCount;

        public static int WriteCount => _writeCount;
    }

    internal class ThingCompactSerializer<T> : ICompactSerializer<T>
        where T : IThing, new()
    {
        public T Read(ICompactReader reader)
        {
            ThingCompactSerializer.CountRead();
            return new T
            {
                Name = reader.ReadStringRef(Thing.FieldNames.Name),
                Value = reader.ReadInt(Thing.FieldNames.Value)
            };
        }

        public void Write(ICompactWriter writer, T obj)
        {
            ThingCompactSerializer.CountWrite();
            writer.WriteStringRef(Thing.FieldNames.Name, obj.Name);
            writer.WriteInt(Thing.FieldNames.Value, obj.Value);
        }
    }

    /*
    internal class ThingCompactSerializer : ICompactSerializer<Thing>
    {
        public Thing Read(ICompactReader reader)
        {
            return new Thing
            {
                Name = reader.ReadStringRef(Thing.FieldNames.Name),
                Value = reader.ReadInt(Thing.FieldNames.Value)
            };
        }

        public void Write(ICompactWriter writer, Thing obj)
        {
            writer.WriteStringRef(Thing.FieldNames.Name, obj.Name);
            writer.WriteInt(Thing.FieldNames.Value, obj.Value);
        }
    }
    */

    /*
    internal class ThingCompactableCompactSerializer : ICompactSerializer<ThingCompactableInterface>
    {
        public ThingCompactableInterface Read(ICompactReader reader)
        {
            return new ThingCompactableInterface
            {
                Name = reader.ReadStringRef(Thing.FieldNames.Name),
                Value = reader.ReadInt(Thing.FieldNames.Value)
            };
        }

        public void Write(ICompactWriter writer, ThingCompactableInterface obj)
        {
            writer.WriteStringRef(Thing.FieldNames.Name, obj.Name);
            writer.WriteInt(Thing.FieldNames.Value, obj.Value);
        }
    }

    internal class ThingCompactableWithTypeNameCompactSerializer : ICompactSerializer<ThingCompactableInterfaceWithTypeName>
    {
        public ThingCompactableInterfaceWithTypeName Read(ICompactReader reader)
        {
            return new ThingCompactableInterfaceWithTypeName
            {
                Name = reader.ReadStringRef(Thing.FieldNames.Name),
                Value = reader.ReadInt(Thing.FieldNames.Value)
            };
        }

        public void Write(ICompactWriter writer, ThingCompactableInterfaceWithTypeName obj)
        {
            writer.WriteStringRef(Thing.FieldNames.Name, obj.Name);
            writer.WriteInt(Thing.FieldNames.Value, obj.Value);
        }
    }
    */

    internal class CountingReflectionSerializer : ReflectionSerializer
    {
        private static int _readCount;
        private static int _writeCount;

        public static void Reset()
        {
            _readCount = _writeCount = 0;
        }

        public static void CountRead() => Interlocked.Increment(ref _readCount);

        public static void CountWrite() => Interlocked.Increment(ref _writeCount);

        public static int ReadCount => _readCount;

        public static int WriteCount => _writeCount;

        public override object Read(ICompactReader reader)
        {
            CountRead();
            return base.Read(reader);
        }

        public override void Write(ICompactWriter writer, object obj)
        {
            CountWrite();
            base.Write(writer, obj);
        }
    }
}
