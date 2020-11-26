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
