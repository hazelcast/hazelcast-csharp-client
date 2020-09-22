using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hazelcast.Protocol
{
    public static class CodecConstants
    {
#if DEBUG
        private static readonly Dictionary<int, string> MessageNames = new Dictionary<int, string>();

        static CodecConstants()
        {
            var codecTypes = typeof (CodecConstants).Assembly
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
