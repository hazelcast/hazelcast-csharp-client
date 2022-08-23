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

#if NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Collections.Concurrent
{
    internal static class ConcurrentDictionaryExtensions
    {
        // this method exists in netstandard 2.1
        //
        // see discussion at https://github.com/dotnet/runtime/issues/13978
        // and the corresponding PR at https://github.com/dotnet/corefx/pull/1783
        //
        // it is implemented as:
        /*
            if (key == null) ThrowKeyNullException();
            if (addValueFactory == null) throw new ArgumentNullException(nameof(addValueFactory));
            if (updateValueFactory == null) throw new ArgumentNullException(nameof(updateValueFactory));

            int hashcode = _comparer.GetHashCode(key);

            while (true)
            {
                TValue oldValue;
                if (TryGetValueInternal(key, hashcode, out oldValue))
                {
                    // key exists, try to update
                    TValue newValue = updateValueFactory(key, oldValue, factoryArgument);
                    if (TryUpdateInternal(key, hashcode, newValue, oldValue))
                    {
                        return newValue;
                    }
                }
                else
                {
                    // key doesn't exist, try to add
                    TValue resultingValue;
                    if (TryAddInternal(key, hashcode, addValueFactory(key, factoryArgument), false, true, out resultingValue))
                    {
                        return resultingValue;
                    }
                }
            }
        */

        public static TValue AddOrUpdate<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument)
        {
            if (addValueFactory == null) throw new ArgumentNullException(nameof(addValueFactory));
            if (updateValueFactory == null) throw new ArgumentNullException(nameof(updateValueFactory));

            while (true)
            {
                if (dictionary.TryGetValue(key, out var oldValue))
                {
                    // key exists, try to update
                    var newValue = updateValueFactory(key, oldValue, factoryArgument);
                    if (dictionary.TryUpdate(key, newValue, oldValue))
                        return newValue;
                }
                else
                {
                    // key doesn't exist, try to add
                    var newValue = addValueFactory(key, factoryArgument);
                    if (dictionary.TryAdd(key, newValue))
                        return newValue;
                }
            }
        }
    }
}

#endif
