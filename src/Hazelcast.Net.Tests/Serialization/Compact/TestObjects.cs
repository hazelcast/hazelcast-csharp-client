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

        public override string ToString() => $"Thing (Name=\"{Name}\", Value={Value})";
    }

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

        public class ThingWrapperSerializer : ICompactSerializer<ThingWrapper>
        {
            public string TypeName => "thing-wrapper";

            public ThingWrapper Read(ICompactReader reader) => throw new NotImplementedException();

            public void Write(ICompactWriter writer, ThingWrapper value) => throw new NotImplementedException();
        }
    }

    internal class ThingInterfaceCompactSerializer : ICompactSerializer<IThing>
    {
        public string TypeName => "i-thing";

        public IThing Read(ICompactReader reader) => throw new NotImplementedException();

        public void Write(ICompactWriter writer, IThing value) => throw new NotImplementedException();
    }

    internal class ThingCompactSerializer<T> : ICompactSerializer<T>
        where T : IThing, new()
    {
        public ThingCompactSerializer()
            : this("thing")
        { }

        public ThingCompactSerializer(string typename)
        {
            TypeName = typename;
        }

        public string TypeName { get; }

        public T Read(ICompactReader reader) => throw new NotImplementedException();

        public void Write(ICompactWriter writer, T value) => throw new NotImplementedException();
    }
}
