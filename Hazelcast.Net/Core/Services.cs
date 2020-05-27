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
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides a lightweight service container.
    /// </summary>
    /// <remarks>
    /// <para>This class provides a lightweight service container, which is actually more a service
    /// locator, which is an anti-pattern. It is going to be used while we refactor the solution.</para>
    /// <para>The purpose here is to reduce the coupling between namespaces, as well as keeping the usage
    /// of non-CLS-compliant code internal. For instance, Microsoft's logging abstractions are not CLS
    /// compliant. The only way we can provide for users to declare which logger to use, is via this
    /// kind of service provider abstraction, as we cannot expose anything through configuration.</para>
    /// </remarks>
    public static class Services
    {
        // NOTE
        //
        // CreateInstance should be used for things that are created once, as it uses
        // Activator.CreateInstance and is not optimized for fast creation of instance

        public static T CreateInstance<T>(params object[] args)
        {
            var instance = CreateInstance(typeof(T), args);
            if (instance is T t) return t;
            throw new InvalidCastException($"Failed to cast object of type {instance.GetType()} to {typeof(T)}.");
        }

        public static T CreateInstance<T>(Type type, params object[] args)
        {
            var instance = CreateInstance(type, args);
            if (instance is T t) return t;
            throw new InvalidCastException($"Failed to cast object of type {instance.GetType()} to {typeof(T)}.");
        }

        public static T CreateInstance<T>(string typeName, params object[] args)
        {
            var instance = CreateInstance(typeName, args);
            if (instance is T t) return t;
            throw new InvalidCastException($"Failed to cast object of type {instance.GetType()} to {typeof(T)}.");
        }

        public static object CreateInstance(Type type, params object[] args)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return Activator.CreateInstance(type, args);
        }

        public static object CreateInstance(string typeName, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typeName));

            var type = Type.GetType(typeName);
            if (type == null)
                throw new ArgumentException($"Unknown type \"{typeName}\".", nameof(typeName));

            return CreateInstance(type, args);
        }
    }
}
