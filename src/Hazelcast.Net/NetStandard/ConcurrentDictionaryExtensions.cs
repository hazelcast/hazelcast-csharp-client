using System;
using System.Collections.Concurrent;

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