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

using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Metrics
{
    internal static class EnumerableOfMetricsExtensions
    {
        public const char StatSeparator = ',';
        public const char KeyValueSeparator = '=';
        public const char NameSeparator = '.';
        public const char EscapeChar = '\\';

        public static string Serialize(this IEnumerable<Metric> metrics)
        {
            var text = new StringBuilder();

            foreach (var metric in metrics)
            {
                if (text.Length > 0) text.Append(StatSeparator);
                if (metric.Descriptor.Prefix != null)
                    text.Append(metric.Descriptor.Prefix).Append(NameSeparator).AppendEscaped(metric.Descriptor.Name);
                else
                    text.Append(metric.Descriptor.Name);
                text.Append(KeyValueSeparator).Append(metric.StringValue);
            }

            return text.ToString();
        }

        private static StringBuilder AppendEscaped(this StringBuilder text, string value)
        {
            if (value == null) return text;

            foreach (var c in value)
            {
                if (c == StatSeparator || c == KeyValueSeparator || c == NameSeparator || c == EscapeChar)
                    text.Append(EscapeChar);
                text.Append(c);
            }

            return text;
        }
    }
}
