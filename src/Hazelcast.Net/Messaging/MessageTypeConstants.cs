// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Reflection;

namespace Hazelcast.Messaging
{
    // this is a utility class used for debugging, that analyzes the codecs via
    // reflection and builds a table that can map the type of a received message
    // to its actual type name (from the codec).
    //
    // it is only compiled, and used, in DEBUG mode.

    internal static class MessageTypeConstants
    {
#if DEBUG
        private static readonly Dictionary<int, string> MessageNames = new Dictionary<int, string>();

        static MessageTypeConstants()
        {
            var codecTypes = typeof (MessageTypeConstants).Assembly
                .GetTypes()
                .Where(x => x.Namespace != null &&
                            x.Namespace.StartsWith("Hazelcast.Protocol", StringComparison.Ordinal) &&
                            x.Name.EndsWith("Codec", StringComparison.Ordinal));

            var typeOfInt = typeof (int);

            foreach (var codecType in codecTypes)
            {
                var codecConstants = codecType
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(x => x.IsLiteral &&
                                !x.IsInitOnly &&
                                x.FieldType == typeOfInt &&
                                x.Name.EndsWith("MessageType", StringComparison.Ordinal));

                var codecName = codecType.Name;
                codecName = codecName.Substring(0, codecName.Length - "Codec".Length);

                foreach (var codecConstant in codecConstants)
                {
                    var name = codecConstant.Name;
                    var value = (int) codecConstant.GetValue(null);

                    name = name.Substring(0, name.Length - "MessageType".Length);
                    if (name.StartsWith("Event", StringComparison.Ordinal))
                        name = name.Substring("Event".Length);

                    MessageNames[value] = codecName + "." + name;
                }
            }
        }
#endif

        public static string GetMessageTypeName(int type)
        {
#if DEBUG
            return MessageNames.TryGetValue(type, out var name) ? name : "(unknown)";
#else
            return string.Empty;
#endif
        }
    }
}
