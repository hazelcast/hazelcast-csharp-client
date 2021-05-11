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
using System.Linq;
using System.Reflection;
using Hazelcast.Configuration.Binding;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides methods to create instances of services.
    /// </summary>
    /// <remarks>
    /// <para>The provided methods rely on the Activator.CreateInstance method to create
    /// the new instances and are not optimized for performance. It is fine to use them
    /// for e.g. creating singletons when the application starts, but they should not
    /// be used for intensive creation of objects.</para>
    /// </remarks>
    public static class ServiceFactory
    {
        /// <summary>
        /// Creates a new instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="stringArgs">Optional string named arguments for the constructor (can be null).</param>
        /// <param name="paramArgs">Parameter arguments for the constructor.</param>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <para>This method relies on the Activator.CreateInstance or constructor
        /// invocation to create the new instance and is not optimized for performance.
        /// It is fine to use it for e.g. creating singletons when the application starts,
        /// but it should not be used for intensive creation of objects.</para>
        /// </remarks>
        public static T CreateInstance<T>(IDictionary<string, string> stringArgs = null, params object[] paramArgs)
        {
#pragma warning disable CA1508 // Avoid dead conditional code
            // false-positive, https://github.com/dotnet/roslyn-analyzers/issues/3845
            if (paramArgs == null) throw new ArgumentNullException(nameof(paramArgs));
#pragma warning restore CA1508

            try
            {
                return As<T>(CreateInstanceInternal(typeof(T), stringArgs, paramArgs));
            }
            catch (Exception e)
            {
                throw new ServiceFactoryException($"Failed to create an instance of type {typeof(T)}.", e);
            }
        }

        /// <summary>
        /// Creates a new instance of type <paramref name="type"/> as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the returned instance.</typeparam>
        /// <param name="type">The type of the created instance.</param>
        /// <param name="stringArgs">Optional string named arguments for the constructor (can be null).</param>
        /// <param name="paramArgs">Parameter arguments for the constructor.</param>
        /// <returns>A new instance of type <paramref name="type"/> as <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <para>This method relies on the Activator.CreateInstance or constructor
        /// invocation to create the new instance and is not optimized for performance.
        /// It is fine to use it for e.g. creating singletons when the application starts,
        /// but it should not be used for intensive creation of objects.</para>
        /// </remarks>
        public static T CreateInstance<T>(Type type, IDictionary<string, string> stringArgs = null, params object[] paramArgs)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
#pragma warning disable CA1508 // Avoid dead conditional code
            // false-positive, https://github.com/dotnet/roslyn-analyzers/issues/3845
            if (paramArgs == null) throw new ArgumentNullException(nameof(paramArgs));
#pragma warning restore CA1508

            try
            {
                return As<T>(CreateInstanceInternal(type, stringArgs, paramArgs));
            }
            catch (Exception e)
            {
                throw new ServiceFactoryException($"Failed to create an instance of type {type} as {typeof(T)}.", e);
            }
        }

        /// <summary>
        /// Creates a new instance of type <paramref name="typeName"/> as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the returned instance.</typeparam>
        /// <param name="typeName">The name of the type of the created instance.</param>
        /// <param name="stringArgs">Optional string named arguments for the constructor (can be null).</param>
        /// <param name="paramArgs">Parameter arguments for the constructor.</param>
        /// <returns>A new instance of type <paramref name="typeName"/> as <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// <para>This method relies on the Activator.CreateInstance or constructor
        /// invocation to create the new instance and is not optimized for performance.
        /// It is fine to use it for e.g. creating singletons when the application starts,
        /// but it should not be used for intensive creation of objects.</para>
        /// </remarks>
        public static T CreateInstance<T>(string typeName, IDictionary<string, string> stringArgs = null, params object[] paramArgs)
        {
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));
#pragma warning disable CA1508 // Avoid dead conditional code
            // false-positive, https://github.com/dotnet/roslyn-analyzers/issues/3845
            if (paramArgs == null) throw new ArgumentNullException(nameof(paramArgs));
#pragma warning restore CA1508

            try
            {
                return As<T>(CreateInstanceInternal(typeName, stringArgs, paramArgs));
            }
            catch (Exception e)
            {
                throw new ServiceFactoryException($"Failed to create an instance of type {typeName} as {typeof(T)}.", e);
            }
        }

        /// <summary>
        /// Creates a new instance of type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="stringArgs">Optional string named arguments for the constructor (can be null).</param>
        /// <param name="paramArgs">Parameter arguments for the constructor.</param>
        /// <returns>A new instance of type <paramref name="type"/>.</returns>
        /// <remarks>
        /// <para>This method relies on the Activator.CreateInstance or constructor
        /// invocation to create the new instance and is not optimized for performance.
        /// It is fine to use it for e.g. creating singletons when the application starts,
        /// but it should not be used for intensive creation of objects.</para>
        /// </remarks>
        public static object CreateInstance(Type type, IDictionary<string, string> stringArgs = null, params object[] paramArgs)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
#pragma warning disable CA1508 // Avoid dead conditional code
            // false-positive, https://github.com/dotnet/roslyn-analyzers/issues/3845
            if (paramArgs == null) throw new ArgumentNullException(nameof(paramArgs));
#pragma warning restore CA1508

            try
            {
                return CreateInstanceInternal(type, stringArgs, paramArgs);
            }
            catch (Exception e)
            {
                throw new ServiceFactoryException($"Failed to create an instance of type {type}.", e);
            }
        }

        /// <summary>
        /// Creates a new instance of type <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">The name of the type of the instance.</param>
        /// <param name="stringArgs">Optional string named arguments for the constructor (can be null).</param>
        /// <param name="paramArgs">Parameter arguments for the constructor.</param>
        /// <returns>A new instance of type <paramref name="typeName"/>.</returns>
        /// <remarks>
        /// <para>This method relies on the Activator.CreateInstance or constructor
        /// invocation to create the new instance and is not optimized for performance.
        /// It is fine to use it for e.g. creating singletons when the application starts,
        /// but it should not be used for intensive creation of objects.</para>
        /// </remarks>
        public static object CreateInstance(string typeName, IDictionary<string, string> stringArgs = null, params object[] paramArgs)
        {
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));
#pragma warning disable CA1508 // Avoid dead conditional code
            // false-positive, https://github.com/dotnet/roslyn-analyzers/issues/3845
            if (paramArgs == null) throw new ArgumentNullException(nameof(paramArgs));
#pragma warning restore CA1508

            try
            {
                return CreateInstanceInternal(typeName, stringArgs, paramArgs);
            }
            catch (Exception e)
            {
                throw new ServiceFactoryException($"Failed to create an instance of type {typeName}.", e);
            }
        }


        /// <summary>
        /// (internal for tests only)
        /// Casts an object.
        /// </summary>
        internal static T As<T>(object o)
        {
            return o switch
            {
                T t => t,
                null => throw new ArgumentNullException(nameof(o)),
                _ => throw new InvalidCastException($"Failed to cast object of type {o.GetType()} to {typeof (T)}.")
            };
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                throw new ArgumentException($"Unknown type \"{typeName}\".", nameof(typeName));
            return type;
        }

        /// <summary>
        /// (internal for tests only)
        /// Creates an instance.
        /// </summary>
        internal static object CreateInstanceInternal(Type type, IDictionary<string, string> stringArgs, params object[] paramArgs)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
#pragma warning disable CA1508 // Avoid dead conditional code
            // false-positive, https://github.com/dotnet/roslyn-analyzers/issues/3845
            if (paramArgs == null) throw new ArgumentNullException(nameof(paramArgs));
#pragma warning restore CA1508

                // fast: use the empty ctor if no args (will throw if it does not exist)
            if ((stringArgs == null || stringArgs.Count == 0) && paramArgs.Length == 0)
                return Activator.CreateInstance(type);

            var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(x => x.GetParameters().Length);

            List<object> args = null;
            foreach (var ctor in ctors)
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length == 0)
                    return Activator.CreateInstance(type);

                args ??= new List<object>();
                args.Clear();

                var match = true;
                foreach (var parameter in parameters)
                {
                    // ReSharper disable once UseMethodIsInstanceOfType
                    var objectArg = paramArgs.FirstOrDefault(x => parameter.ParameterType.IsAssignableFrom(x.GetType()));
                    if (objectArg != null)
                    {
                        args.Add(objectArg);
                        continue;
                    }

                    if (stringArgs != null &&
                        stringArgs.TryGetValue(parameter.Name, out var stringArg) &&
                        ConfigurationBinder.TryConvertValue(parameter.ParameterType, stringArg, "", out var value, out _))
                    {
                        args.Add(value);
                        continue;
                    }

                    match = false;
                    break;
                }

                if (match)
                    return ctor.Invoke(args.ToArray());
            }

            // we know this throw - but then the exceptions are consistent
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// (internal for tests only)
        /// Creates an instance.
        /// </summary>
        internal static object CreateInstanceInternal(string typeName, IDictionary<string, string> stringArgs, params object[] paramArgs)
        {
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));
            return CreateInstanceInternal(GetType(typeName), stringArgs, paramArgs);
        }
    }
}
