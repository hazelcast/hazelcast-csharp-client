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

        // Set methods are not conditional (since they have a non-void return type)
        // but that does not matter because they will be invoked from within the
        // HConsole.Configure() method, which *is* conditional, so they won't be
        // compiled either when HZ_CONSOLE is not defined.

        /// <summary>
        /// Sets the default options (ie for <c>object</c>).
        /// </summary>
        /// <param name="configure">An action to configure the options.</param>
        /// <returns>The console options.</returns>
        public HConsoleOptions Set(Action<HConsoleTargetOptions> configure) => Set<object>(configure);

        /// <summary>
        /// Sets the options for a specific object.
        /// </summary>
        /// <param name="target">The object.</param>
        /// <param name="configure">An action to configure the options.</param>
        /// <returns>The console options.</returns>
        public HConsoleOptions Set(object target, Action<HConsoleTargetOptions> configure)
        {
#if HZ_CONSOLE
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            if (!_targetConfigs.TryGetValue(target, out var info))
                info = _targetConfigs[target] = new HConsoleTargetOptions();
            configure(info);
#endif
            return this;
        }

        /// <summary>
        /// Sets the options for an object type.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="configure">An action to configure the options.</param>
        /// <returns>The console options.</returns>
        public HConsoleOptions Set(Type type, Action<HConsoleTargetOptions> configure)
        {
#if HZ_CONSOLE
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            if (!_typeConfigs.TryGetValue(type, out var info))
                info = _typeConfigs[type] = new HConsoleTargetOptions();
            configure(info);
#endif
            return this;
        }

        public HConsoleOptions Set(string typename, Action<HConsoleTargetOptions> configure)
        {
#if HZ_CONSOLE
            var type = Type.GetType(typename);
            if (type == null) throw new ArgumentException("Invalid type name.", nameof(typename));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            if (!_typeConfigs.TryGetValue(type, out var info))
                info = _typeConfigs[type] = new HConsoleTargetOptions();
            configure(info);
#endif
            return this;
        }

        /// <summary>
        /// Sets the options for an object type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="configure">An action to configure the options.</param>
        /// <returns>The console options.</returns>
        public HConsoleOptions Set<TObject>(Action<HConsoleTargetOptions> configure)
        {
#if HZ_CONSOLE
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var type = typeof(TObject);
            if (!_typeConfigs.TryGetValue(type, out var info))
                info = _typeConfigs[type] = new HConsoleTargetOptions();
            configure(info);
#endif
            return this;
        }

        /// <summary>
        /// Clears the default options (ie for object).
        /// </summary>
        /// <returns>The console options.</returns>
        public HConsoleOptions Clear() => Clear<object>();

        /// <summary>
        /// Clears the options for a specific object.
        /// </summary>
        /// <param name="target">The object.</param>
        /// <returns>The console options.</returns>
        public HConsoleOptions Clear(object target)
        {
#if HZ_CONSOLE
            if (target == null) throw new ArgumentNullException(nameof(target));
            _targetConfigs.Remove(target);
#endif
            return this;
        }

        /// <summary>
        /// Clears the options for an object type.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>The console options.</returns>
        public HConsoleOptions Clear(Type type)
        {
#if HZ_CONSOLE
            if (type == null) throw new ArgumentNullException(nameof(type));
            _typeConfigs.Remove(type);
#endif
            return this;
        }

        /// <summary>
        /// Clears the options for an object type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>The console options.</returns>
        public HConsoleOptions Clear<TObject>()
        {
#if HZ_CONSOLE
            _typeConfigs.Remove(typeof (TObject));
#endif
            return this;
        }

        /// <summary>
        /// Clears the options entirely.
        /// </summary>
        /// <returns>The console options.</returns>
        public HConsoleOptions ClearAll()
        {
#if HZ_CONSOLE
            _typeConfigs.Clear();
            _targetConfigs.Clear();
#endif
            return this;
        }

#if HZ_CONSOLE
        /// <summary>
        /// Gets the options for an object.
        /// </summary>
        /// <param name="target">The object.</param>
        /// <returns>The complete options that apply to the specified object.</returns>
        public HConsoleTargetOptions Get(object target)
        {
            if (_targetConfigs.TryGetValue(target, out var config))
                config = config.Clone();
            else
                config = new HConsoleTargetOptions();

            var type = target.GetType();
            while (type != null && !config.IsComplete)
            {
                if (_typeConfigs.TryGetValue(type, out var c)) config = config.Merge(c);
                type = type.BaseType;
            }

            if (!config.IsComplete)
                config = config.Merge(HConsoleTargetOptions.Default);

            return config;
        }
#endif
    }
}
