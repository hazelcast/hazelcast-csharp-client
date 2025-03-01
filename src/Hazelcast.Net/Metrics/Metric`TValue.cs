﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Globalization;

namespace Hazelcast.Metrics
{
    // a metric with its value
    internal class Metric<TValue> : Metric
    {
        public TValue Value { get; set; }

        public override string StringValue
        {
            get
            {
                // beware! it is important to serialize values in the Java-expected way!
#pragma warning disable CA1308 // Normalize strings to uppercase - well, no
                return Value switch
                {
                    double d => d.ToString(CultureInfo.InvariantCulture),
                    null     => "",
                    bool b   => b.ToString().ToLowerInvariant(),
                    _        => Value.ToString()
                };
#pragma warning restore CA1308
            }
        }

        public override string ToString()
        {
            return $"{Descriptor} = ({typeof (TValue).Name}) {StringValue}";
        }
    }
}
