// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    internal static class MetricsExtensions
    {
        private const char MetricSeparator = ',';
        private const char KeyValueSeparator = '=';
        private const char NameSeparator = '.';
        private const char EscapeChar = '\\';

        public static StringBuilder AppendEscape(this StringBuilder text, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return text;

            foreach (var c in value)
            {
                if (c == MetricSeparator || c == KeyValueSeparator || c == NameSeparator || c == EscapeChar)
                    text.Append(EscapeChar);
                text.Append(c);
            }

            return text;
        }

        private static StringBuilder AppendMetric(this StringBuilder text, Metric metric)
        {
            if (metric == null) return text;

            var descriptor = metric.Descriptor;

            if (descriptor.AttributePrefixes != null)
                foreach (var prefix in descriptor.AttributePrefixes)
                    text.AppendEscape(prefix).Append(NameSeparator);

            else if (descriptor.Prefix != null)
                text.Append(descriptor.Prefix).Append(NameSeparator);

            text.Append(descriptor.Name).Append(KeyValueSeparator).Append(metric.StringValue);

            return text;
        }

        public static string Serialize(this IEnumerable<Metric> metrics)
        {
            var text = new StringBuilder();

            foreach (var metric in metrics)
            {
                if (text.Length > 0) text.Append(MetricSeparator);
                text.AppendMetric(metric);
            }

            return text.ToString();
        }
    }
}
