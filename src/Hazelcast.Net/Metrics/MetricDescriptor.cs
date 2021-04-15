﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Metrics
{
    // a metric descriptor for values of a given type
    internal class MetricDescriptor<TValue> : IMetricDescriptor
    {
        private IDictionary<string, string> _tags;

        public MetricDescriptor(string name)
        {
            Name = name;
        }

        public MetricDescriptor(string prefix, string name)
            : this(name)
        {
            Prefix = prefix;
        }

        public string Prefix { get; set; }

        public string Name { get; set; }

        public string DiscriminatorKey { get; set; }

        public string DiscriminatorValue { get; set; }

        public MetricUnit Unit { get; set; }

        public IDictionary<string, string> Tags => _tags ??= new Dictionary<string, string>();

        public int TagCount => _tags?.Count ?? 0;

        public MetricDescriptor<TValue> WithUnit(MetricUnit unit)
        {
            Unit = unit;
            return this;
        }

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

        // "excluded targets" are not supported
    }
}