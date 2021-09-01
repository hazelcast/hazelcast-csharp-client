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

#if HZ_CONSOLE
using System.Collections.Generic;
#else
// ReSharper disable UnusedTypeParameter
#pragma warning disable CA1801 // Review unused parameters
#pragma warning disable CA1822 // Mark members as static
#endif

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the options for the console.
    /// </summary>
#if HZ_CONSOLE_PUBLIC
    public
#else
    internal
#endif
    class HConsoleOptions
    {
#if HZ_CONSOLE
        private readonly Dictionary<object, HConsoleTargetOptions> _targetConfigs = new Dictionary<object, HConsoleTargetOptions>();
        private readonly Dictionary<Type, HConsoleTargetOptions> _typeConfigs = new Dictionary<Type, HConsoleTargetOptions>();
#endif

        // Configure methods are not conditional (since they have a non-void return type)
        // but that does not matter because they will be invoked from within the
        // HConsole.Configure() method, which *is* conditional, so they won't be
        // compiled either when HZ_CONSOLE is not defined.

        #region Configure

        /// <summary>
        /// Configures default options.
        /// </summary>
        /// <returns>The default options to configure.</returns>
        public HConsoleTargetOptions Configure()
#if HZ_CONSOLE
            => Configure<object>();
#else
            => default;
#endif

        /// <summary>
        /// Configures options for a source type.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <returns>The options for the source type.</returns>
        public HConsoleTargetOptions Configure<TSource>()
#if HZ_CONSOLE
            => Configure(typeof (TSource));
#else
            => default;
#endif

        /// <summary>
        /// Configures options for a source type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <returns>The options for the source type.</returns>
        public HConsoleTargetOptions Configure(Type sourceType)
        {
#if HZ_CONSOLE
            if (!_typeConfigs.TryGetValue(sourceType ?? throw new ArgumentNullException(nameof(sourceType)), out var info))
                info = _typeConfigs[sourceType] = new HConsoleTargetOptions(this);
            return info;
#else
            return default;
#endif
        }

        /// <summary>
        /// Configures options for a source type.
        /// </summary>
        /// <param name="sourceTypeName">The name of the source type.</param>
        /// <returns>The options for the source type.</returns>
        public HConsoleTargetOptions Configure(string sourceTypeName)
#if HZ_CONSOLE
            => Configure(Type.GetType(sourceTypeName) ?? throw new ArgumentException($"Could not find type {sourceTypeName}.", nameof(sourceTypeName)));
#else
            => default;
#endif

        /// <summary>
        /// Configures options for a source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The options for the source object.</returns>
        public HConsoleTargetOptions Configure(object source)
        {
#if HZ_CONSOLE
            if (!_targetConfigs.TryGetValue(source ?? throw new ArgumentNullException(nameof(source)), out var info))
                info = _targetConfigs[source] = new HConsoleTargetOptions(this);
            return info;
#else
            return default;
#endif
        }

        /// <summary>
        /// Clears the default options.
        /// </summary>
        /// <returns>The console options.</returns>
        public HConsoleOptions Clear() => Clear<object>();

        /// <summary>
        /// Clears the options for a source type.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <returns>The options.</returns>
        public HConsoleOptions Clear<TSource>()
        {
#if HZ_CONSOLE
            _typeConfigs.Remove(typeof(TSource));
#endif
            return this;
        }

        /// <summary>
        /// Clears the options for a source type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <returns>The options.</returns>
        public HConsoleOptions Clear(Type sourceType)
        {
#if HZ_CONSOLE
            _typeConfigs.Remove(sourceType ?? throw new ArgumentNullException(nameof(sourceType)));
#endif
            return this;
        }

        /// <summary>
        /// Clears the options for a source type.
        /// </summary>
        /// <param name="sourceTypeName">The name of the source type.</param>
        /// <returns>The options.</returns>
        public HConsoleOptions Clear(string sourceTypeName)
        {
#if HZ_CONSOLE
            _typeConfigs.Remove(Type.GetType(sourceTypeName) ?? throw new ArgumentException($"Could not find type {sourceTypeName}.", nameof(sourceTypeName)));
#endif
            return this;
        }

        /// <summary>
        /// Clears the options for a specific object.
        /// </summary>
        /// <param name="target">The object.</param>
        /// <returns>The console options.</returns>
        public HConsoleOptions Clear(object target)
        {
#if HZ_CONSOLE
            _targetConfigs.Remove(target ?? throw new ArgumentNullException(nameof(target)));
#endif
            return this;
        }

        /// <summary>
        /// Clears all options.
        /// </summary>
        /// <returns>The options.</returns>
        public HConsoleOptions ClearAll()
        {
#if HZ_CONSOLE
            _typeConfigs.Clear();
            _targetConfigs.Clear();
#endif
            return this;
        }

        #endregion

#if HZ_CONSOLE
        /// <summary>
        /// Gets the options for a source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The complete options that apply to the specified source object.</returns>
        public HConsoleTargetOptions GetOptions(object source)
        {
            if (_targetConfigs.TryGetValue(source, out var config))
                config = config.Clone();
            else
                config = new HConsoleTargetOptions(this);

            var type = source.GetType();
            while (type != null && !config.IsComplete)
            {
                if (_typeConfigs.TryGetValue(type, out var typeConfig)) config = config.Merge(typeConfig);
                if (type.IsGenericType && _typeConfigs.TryGetValue(type.GetGenericTypeDefinition(), out var gendefConfig)) config = config.Merge(gendefConfig);
                type = type.BaseType;
            }

            if (!config.IsComplete)
                config = config.Complete();

            return config;
        }
#endif
    }
}
