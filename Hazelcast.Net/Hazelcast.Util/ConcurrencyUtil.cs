namespace Hazelcast.Util
{
    public class ConcurrencyUtil
    {
        //public static V GetOrPutSynchronized<K, V>(ConcurrentMap<K, V> map, K key, object mutex, ConstructorFunction<K, V> func)
        //{
        //    if (mutex == null)
        //    {
        //        throw new ArgumentNullException();
        //    }
        //    V value = map.Get(key);
        //    if (value == null)
        //    {
        //        lock (mutex)
        //        {
        //            value = map.Get(key);
        //            if (value == null)
        //            {
        //                value = func.CreateNew(key);
        //                map.Put(key, value);
        //            }
        //        }
        //    }
        //    return value;
        //}

        //public static V GetOrPutIfAbsent<K, V>(ConcurrentMap<K, V> map, K key, ConstructorFunction<K, V> func)
        //{
        //    V value = map.Get(key);
        //    if (value == null)
        //    {
        //        value = func.CreateNew(key);
        //        V current = map.PutIfAbsent(key, value);
        //        value = current == null ? value : current;
        //    }
        //    return value;
        //}
    }
}