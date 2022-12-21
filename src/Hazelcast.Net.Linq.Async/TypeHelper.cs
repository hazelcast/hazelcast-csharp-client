// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Reflection;

namespace Hazelcast.Linq
{
    internal static class TypeHelper
    {
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
        /// <summary>
        /// Gets the element type given the sequence type.
        /// If the type is not a sequence, returns the type itself.
        /// </summary>
        public static Type GetElementType(Type sequenceType)
        {
            var iEnumerable = FindIEnumerable(sequenceType);
            return iEnumerable is null ? sequenceType : iEnumerable.GetTypeInfo().GenericTypeArguments[0];
        }
        
        /// <summary>
        /// Finds the type's implemented <see cref="IEnumerable{T}"/> type.
        /// </summary>
        public static Type? FindIEnumerable(this Type? type)
        {
            if (type is null || type == typeof(string))
                return null;

            if (type.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType()!);

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                foreach (var arg in typeInfo.GenericTypeArguments)
                {
                    var iEnumerable = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (iEnumerable.GetTypeInfo().IsAssignableFrom(typeInfo))
                    {
                        return iEnumerable;
                    }
                }
            }

            foreach (var impInterface in typeInfo.ImplementedInterfaces)
            {
                var iEnumerable = FindIEnumerable(impInterface);
                if (iEnumerable != null) return iEnumerable;
            }

            if (typeInfo.BaseType != null && typeInfo.BaseType != typeof(object))
            {
                return FindIEnumerable(typeInfo.BaseType);
            }

            return null;
        }

        public static bool IsPrimitiveType(this Type t)
        {
            return t.IsPrimitive || t == typeof(Decimal) || t == typeof(String);
        }
    }
    
    
}
