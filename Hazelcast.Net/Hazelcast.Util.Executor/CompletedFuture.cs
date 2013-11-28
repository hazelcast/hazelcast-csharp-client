using System;
using System.Threading.Tasks;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;


namespace Hazelcast.Util.Executor
{
	
    //public sealed class CompletedTask<V> : Task<V>
    //{
    //    private readonly ISerializationService serializationService;

    //    private readonly object value;

    //    public CompletedTask(ISerializationService serializationService, object value) : base()
    //    {
    //        this.serializationService = serializationService;
    //        this.value = value;
    //    }

    //    /// <exception cref="System.Exception"></exception>
    //    /// <exception cref="Hazelcast.Net.Ext.ExecutionException"></exception>
    //    public V Get()
    //    {
    //        object @object = value;
    //        if (@object is Data)
    //        {
    //            @object = serializationService.ToObject((Data)@object);
    //        }
    //        if (@object is Exception)
    //        {
    //            if (@object is ExecutionException)
    //            {
    //                throw (ExecutionException)@object;
    //            }
    //            if (@object is Exception)
    //            {
    //                throw (Exception)@object;
    //            }
    //            throw new ExecutionException((Exception)@object);
    //        }
    //        return (V)@object;
    //    }

    //    /// <exception cref="System.Exception"></exception>
    //    /// <exception cref="Hazelcast.Net.Ext.ExecutionException"></exception>
    //    /// <exception cref="TimeoutException"></exception>
    //    public V Get(long timeout, TimeUnit unit)
    //    {
    //        return Get();
    //    }

    //    public bool Cancel(bool mayInterruptIfRunning)
    //    {
    //        return false;
    //    }

    //    public bool IsCancelled()
    //    {
    //        return false;
    //    }

    //    public bool IsDone()
    //    {
    //        return true;
    //    }
    //}
}
