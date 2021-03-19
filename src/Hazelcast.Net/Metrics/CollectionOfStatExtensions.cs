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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Hazelcast.Metrics
{
    internal static class CollectionOfStatExtensions
    {
        public static void AddEmptyStat(this ICollection<IStat> stats, string name)
            => stats.AddEmptyStat(null, name);

        public static void AddEmptyStat(this ICollection<IStat> stats, string prefix, string name)
            => stats.AddStat(prefix, name, "");

        public static void AddStat(this ICollection<IStat> stats, string name, string value)
            => stats.AddStat(null, name, value);

        public static void AddStat(this ICollection<IStat> stats, string prefix, string name, string value)
            => stats.Add(new StaticStat(prefix, name, value));

        public static void AddStat<T>(this ICollection<IStat> stats, string name, T value)
            => stats.AddStat(null, name, value);

        public static void AddStat<T>(this ICollection<IStat> stats, string prefix, string name, T value)
            => stats.Add(new StaticStat(prefix, name, value.ToInvariantString()));

        public static void AddStat(this ICollection<IStat> stats, string name, Func<string> producer)
            => stats.AddStat(null, name, producer);

        public static void AddStat(this ICollection<IStat> stats, string prefix, string name, Func<string> producer)
            => stats.Add(new LiveStat(prefix, name, producer));

        public static void AddStat<T>(this ICollection<IStat> stats, string name, Func<T> producer)
            => stats.AddStat(null, name, producer);

        public static void AddStat<T>(this ICollection<IStat> stats, string prefix, string name, Func<T> producer)
            => stats.Add(new LiveStat(prefix, name, () => producer().ToInvariantString()));

        private static string ToInvariantString<T>(this T value)
        {
            switch (value)
            {
                case double d: return d.ToString(CultureInfo.InvariantCulture);
                default: return value.ToString();
            }
        }

        public const char StatSeparator = ',';
        public const char KeyValueSeparator = '=';
        public const char NameSeparator = '.';
        public const char EscapeChar = '\\';

        public static string Serialize(this ICollection<IStat> stats)
        {
            // the statistics string has the following format:
            // [key]=[value],[key]=[value],...
            // FIXME where [key] and [value] have ',', '=', '.', '\' backslash-escaped

            var text = new StringBuilder();
            foreach (var stat in stats) //text.Append(stat);
            {
                string value;

                try
                {
                    value = stat.GetValue();
                }
                catch
                {
                    // ignore, send an empty stat
                    // FIXME shall we log?
                    value = "";
                }

                if (text.Length > 0) text.Append(StatSeparator);
                if (stat.Prefix != null) text.AppendEscaped(stat.Prefix).Append(NameSeparator);
                text.AppendEscaped(stat.Name).Append(KeyValueSeparator).AppendEscaped(value);
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