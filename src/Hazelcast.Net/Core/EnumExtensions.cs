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
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="Enum"/>.
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        /// Determines whether one or more bit fields are set in the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">This instance value.</param>
        /// <param name="flags">An enumeration value.</param>
        /// <returns><c>true</c> if all the bit field or bit fields that are set in flag are also set in the current instance; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>This extension methods works for enumerations backed by an <see cref="int"/> value, or any smaller value.
        /// No test is performed on <c>value.GetTypeCode()</c> and therefore results for enumerations backed, by example,
        /// by a <see cref="long"/> are unspecified.</para>
        /// <para>This is a convenient replacement for <see cref="Enum.HasFlag"/> which is way slower.</para>
        /// </remarks>
        public static bool HasAll<T>(this T value, T flags) where T : struct, Enum
        {
            var cflags = Converter<T>.ToLong(flags);
            return (Converter<T>.ToLong(value) & cflags) == cflags;
        }

        /// <summary>
        /// Determines whether one or more bit fields are set in the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">This instance value.</param>
        /// <param name="flags">An enumeration value.</param>
        /// <returns><c>true</c> if any of the bit field or bit fields that are set in flag are also set in the current instance; otherwise, <c>false</c>.</returns>
        public static bool HasAny<T>(this T value, T flags) where T : struct, Enum
            => (Converter<T>.ToLong(value) & Converter<T>.ToLong(flags)) > 0;

        /// <summary>
        /// Determines whether one or more bit fields are not set in the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">This instance value.</param>
        /// <param name="flags">An enumeration value.</param>
        /// <returns><c>true</c> if none of the bit field or bit fields that are set in flag are also set in the current instance; otherwise, <c>false</c>.</returns>
        public static bool HasNone<T>(this T value, T flags) where T : struct, Enum
            => (Converter<T>.ToLong(value) & Converter<T>.ToLong(flags)) == 0;

        // An enum declaration may explicitly declare an underlying type of byte, sbyte, short, ushort, int, uint, long or ulong.
        // see https://devblogs.microsoft.com/premier-developer/dissecting-new-generics-constraints-in-c-7-3/
        //
        // var i = (int) enumValue is fine & fast, as long as we are not using generics
        // we *could* write extensions for all our enums, that's be fast code but... tedious?
        // = (int) (IConvertible) enumValue -> implies boxing/unboxing
        // code below is a good compromise

        private static class Converter<TEnum> where TEnum : struct, Enum
        {
            public static readonly Func<TEnum, long> ToLong = CreateToLong();

            private static Func<TEnum, long> CreateToLong()
            {
                var parameter = Expression.Parameter(typeof (TEnum));
                //var convert = Expression.ConvertChecked(parameter, typeof (long));
                var convert = Expression.Convert(parameter, typeof(long));
                return Expression.Lambda<Func<TEnum, long>>(convert, parameter).Compile();

                // code below would work in NetStandard 2.1 but only benchmarking could determine whether it's worth it

                /*
                var method = new DynamicMethod(
                    name: "ConvertToLong",
                    returnType: typeof(long),
                    parameterTypes: new[] { typeof(TEnum) },
                    m: typeof(EnumExtensions).Module,
                    skipVisibility: true);

                ILGenerator ilGen = method.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Conv_I8);
                ilGen.Emit(OpCodes.Ret);

                return (Func<TEnum, long>) method.CreateDelegate(typeof(Func<TEnum, long>));
                */
            }
        }
    }
}
