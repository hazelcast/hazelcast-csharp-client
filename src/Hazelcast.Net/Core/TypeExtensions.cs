using System;
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines whether this type is a nullable type.
        /// </summary>
        /// <param name="type">This type.</param>
        /// <returns><c>true</c> if this type is a nullable type; otherwise <c>false</c>.</returns>
        public static bool IsNullableType(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        /// <summary>
        /// Returns a string representing this type in C# style.
        /// </summary>
        /// <param name="type">This type.</param>
        /// <param name="fqn">Whether to keep fully-qualified name or shorten them.</param>
        /// <returns>A string representing this type in C# style.</returns>
        public static string ToCsString(this Type type, bool fqn = false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var s = type.FullName;
            if (s == null) return type.ToString();
            if (!type.IsGenericType) return NonGenericToCsString(s, fqn);

            var p = s.IndexOf('`', StringComparison.Ordinal);
            var def = NonGenericToCsString(s.Substring(0, p), fqn);
            var args = type.GetGenericArguments().Select(x => x.ToCsString(fqn));

            return def + "<" + string.Join(", ", args) + ">";
        }

        private static string NonGenericToCsString(string fullname, bool fqn)
        {
            if (TypesMap.TryGetValue(fullname, out var typeName))
                return typeName;

            var name = fullname;
            if (!fqn)
            {
                var pos = name.LastIndexOf('.');
                if (pos >= 0) name = name.Substring(pos + 1);
            }
            return name.Replace('+', '.');
        }

        private static readonly IDictionary<string, string> TypesMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "System.Int16", "short" },
            { "System.Int32", "int" },
            { "System.Int64", "long" },
            { "System.String", "string" },
            { "System.Object", "object" },
            { "System.Boolean", "bool" },
            { "System.Void", "void" },
            { "System.Char", "char" },
            { "System.Byte", "byte" },
            { "System.UInt16", "ushort" },
            { "System.UInt32", "uint" },
            { "System.UInt64", "ulong" },
            { "System.SByte", "sbyte" },
            { "System.Single", "float" },
            { "System.Double", "double" },
            { "System.Decimal", "decimal" }
        };
    }
}
