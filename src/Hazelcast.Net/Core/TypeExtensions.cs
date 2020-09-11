using System;

namespace Hazelcast.Core
{
    public static class TypeExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }
    }
}
