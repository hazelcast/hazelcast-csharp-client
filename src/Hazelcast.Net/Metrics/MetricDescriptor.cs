// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Metrics
{
    internal static class MetricDescriptor
    {
        public static MetricDescriptor<TValue> Create<TValue>(string name, MetricUnit unit = MetricUnit.None)
            => MetricDescriptor<TValue>.Create(name, unit);

        public static MetricDescriptor<TValue> Create<TValue>(string prefix, string name, MetricUnit unit = MetricUnit.None)
            => MetricDescriptor<TValue>.Create(prefix, name, unit);
    }

    // a metric descriptor for values of a given type
    internal class MetricDescriptor<TValue> : IMetricDescriptor
    {
        private IDictionary<string, string> _tags;

        private MetricDescriptor(string prefix, string name)
        {
            Prefix = prefix;
            Name = name;
        }

        public static MetricDescriptor<TValue> Create(string name, MetricUnit unit)
            => new MetricDescriptor<TValue>(null, name) { Unit = unit };

        public static MetricDescriptor<TValue> Create(string prefix, string name, MetricUnit unit)
            => new MetricDescriptor<TValue>(prefix, name) { Unit = unit };

        public string Prefix { get; }

        public string[] AttributePrefixes { get; set; }

        public string Name { get; }

        public string DiscriminatorKey { get; set; }

        public string DiscriminatorValue { get; set; }

        public MetricUnit Unit { get; private set; }

        public IDictionary<string, string> Tags => _tags ??= new Dictionary<string, string>();

        public int TagCount => _tags?.Count ?? 0;

        public MetricDescriptor<TValue> WithDiscriminator(string key, string value)
        {
            DiscriminatorKey = key;
            DiscriminatorValue = value;
            return this;
        }

        public MetricDescriptor<TValue> WithTag(string key, string value)
        {
            Tags[key] = value;
            return this;
        }

        public MetricDescriptor<TValue> WithAttributePrefixes(params string[] prefixes)
        {
            AttributePrefixes = prefixes;
            return this;
        }

        // "excluded targets" are not supported

        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append('[');

            text.Append("metric=");

            if (Prefix != null)
            {
                text.Append(Prefix);
                text.Append('.');
            }

            text.Append(Name);

            text.Append(',');
            text.Append("unit=");
            text.Append(Unit);

            if (DiscriminatorKey != null)
            {
                text.Append(',');
                text.Append(DiscriminatorKey);
                text.Append('=');
                text.Append(DiscriminatorValue);
            }

            text.Append(']');
            return text.ToString();
        }
    }
}
