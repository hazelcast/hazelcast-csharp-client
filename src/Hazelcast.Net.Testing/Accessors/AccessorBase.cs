// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Reflection;

namespace Hazelcast.Testing.Accessors
{
    // TODO [Oleksii] consider adding some caching/delegates
    public abstract class AccessorBase<T>
    {
        public T Instance { get; }

        protected AccessorBase(T instance)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        protected void SetField<TField>(string fieldName, TField value)
        {
            var member = GetFieldInfoOrThrow(fieldName);
            member.SetValue(Instance, value);
        }

        protected TField GetField<TField>(string fieldName)
        {
            var member = GetFieldInfoOrThrow(fieldName);
            return (TField)member.GetValue(Instance);
        }

        protected void SetProperty<TField>(string fieldName, TField value)
        {
            var member = GetPropertyInfoOrThrow(fieldName);
            member.SetValue(Instance, value);
        }

        protected TField GetProperty<TField>(string fieldName)
        {
            var member = GetPropertyInfoOrThrow(fieldName);
            return (TField)member.GetValue(Instance);
        }

        private FieldInfo GetFieldInfoOrThrow(string fieldName)
        {
            var type = Instance.GetType();
            return type.GetField(fieldName, BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?? throw new MissingFieldException(type.FullName, fieldName);
        }

        private PropertyInfo GetPropertyInfoOrThrow(string fieldName)
        {
            var type = Instance.GetType();
            return type.GetProperty(fieldName, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?? throw new MissingFieldException(type.FullName, fieldName);
        }
    }
}
