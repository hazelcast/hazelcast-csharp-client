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

using Hazelcast.Serialization.Compact;

namespace Hazelcast.Tests.Serialization.Compact
{
    internal class Thing
    {
        public const string TypeName = "thing";

        public static class FieldNames
        {
            public const string Name = "name";
            public const string Value = "mehalue"; // 'value' is a keyword?!
        }

        public string Name { get; set; }

        public int Value { get; set; }
    }

    internal class ThingCompactableInterface : ICompactable<ThingCompactableInterface>
    {
        public const string TypeName = "cthing1";

        public string Name { get; set; }

        public int Value { get; set; }

        private static readonly ICompactSerializer<ThingCompactableInterface> Serializer = new ThingCompactableCompactSerializer();

        ICompactSerializer<ThingCompactableInterface> ICompactable<ThingCompactableInterface>.GetSerializer() => Serializer;
    }

    internal class ThingCompactableInterfaceWithTypeName : ICompactable<ThingCompactableInterfaceWithTypeName>, ICompactableWithTypeName
    {
        public const string TypeName = "cthing2";

        public string Name { get; set; }

        public int Value { get; set; }

        private static readonly ICompactSerializer<ThingCompactableInterfaceWithTypeName> Serializer = new ThingCompactableWithTypeNameCompactSerializer();

        ICompactSerializer<ThingCompactableInterfaceWithTypeName> ICompactable<ThingCompactableInterfaceWithTypeName>.GetSerializer() => Serializer;

        string ICompactableWithTypeName.TypeName => TypeName;
    }

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
}
