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
using System.Threading;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;

namespace Hazelcast.Tests.Serialization.Compact
{
    internal interface IReadWriteObjectsFromIObjectDataInputOutput : IReadObjectsFromObjectDataInput, IWriteObjectsToObjectDataOutput
    { }

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

        public override string ToString() => $"Thing (Name=\"{Name}\", Value={Value})";
    }

    internal class ThingExtend : Thing
    { }

    internal class DifferentThing : IThing
    {
        public const string TypeName = "different_thing";

        public static class FieldNames
        {
            public const string Name = "name";
            public const string Value = "value";
        }

        public string? Name { get; set; }

        public int Value { get; set; }

        public override string ToString() => $"DifferentThing (Name=\"{Name}\", Value={Value})";
    }

    internal class ThingWrapper
    {
        public IThing? Thing { get; set; }

        public class ThingWrapperSerializer : CompactSerializerBase<ThingWrapper>
        {
            public override string TypeName => "thing-wrapper";

            public override ThingWrapper Read(ICompactReader reader)
            {
                return new ThingWrapper { Thing = reader.ReadCompact<IThing>("thing") };
            }

            public override void Write(ICompactWriter writer, ThingWrapper value)
            {
                writer.WriteCompact("thing", value.Thing);
            }
        }
    }

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

    internal class ThingInterfaceCompactSerializer : CompactSerializerBase<IThing>
    {
        public override string TypeName => "i-thing";

        public override void Write(ICompactWriter writer, IThing value)
        {
            writer.WriteString("_type", value.GetType().Name);
            writer.WriteString("name", value.Name);
            writer.WriteInt32("value", value.Value);
        }

        public override IThing Read(ICompactReader reader)
        {
            var t = reader.ReadString("_type");
            if (t == nameof(Thing))
                return new Thing { Name = reader.ReadString("name"), Value = reader.ReadInt32("value") };
            if (t == nameof(DifferentThing))
                return new DifferentThing { Name = reader.ReadString("name"), Value = reader.ReadInt32("value") };
            throw new NotSupportedException();
        }
    }

    internal class ThingCompactSerializer<T> : CompactSerializerBase<T>
        where T : IThing, new()
    {
        public ThingCompactSerializer()
            : this("thing")
        { }

        public ThingCompactSerializer(string typeName)
        {
            TypeName = typeName;
        }

        public override string TypeName { get; }

        public override T Read(ICompactReader reader)
        {
            ThingCompactSerializer.CountRead();
            return new T
            {
                Name = reader.ReadString(Thing.FieldNames.Name),
                Value = reader.ReadInt32(Thing.FieldNames.Value)
            };
        }

        public override void Write(ICompactWriter writer, T obj)
        {
            ThingCompactSerializer.CountWrite();
            writer.WriteString(Thing.FieldNames.Name, obj.Name);
            writer.WriteInt32(Thing.FieldNames.Value, obj.Value);
        }
    }

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
