using System;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util.Executor;


namespace Hazelcast.Util.Executor
{
	
	public sealed class PoolExecutorThreadFactory //: AbstractExecutorThreadFactory
	{
        //private readonly string threadNamePrefix;

        //private readonly AtomicInteger idGen = new AtomicInteger(0);

        //private readonly Queue<int> idQ = new LinkedBlockingQueue<int>(1000);

        //public PoolExecutorThreadFactory(ThreadGroup threadGroup, string threadNamePrefix) : base(threadGroup)
        //{
        //    // to reuse previous thread IDs
        //    this.threadNamePrefix = threadNamePrefix;
        //}

        //protected internal override Hazelcast.Net.Ext.Thread CreateThread(Runnable r)
        //{
        //    int id = idQ.Poll();
        //    if (id == null)
        //    {
        //        id = idGen.IncrementAndGet();
        //    }
        //    string name = threadNamePrefix + id;
        //    return new PoolExecutorThreadFactory.ManagedThread(this, r, name, id);
        //}

        //private class ManagedThread : Hazelcast.Net.Ext.Thread
        //{
        //    protected internal readonly int id;

        //    public ManagedThread(PoolExecutorThreadFactory _enclosing, Runnable target, string name, int id) : base(this._enclosing.threadGroup, target, name)
        //    {
        //        this._enclosing = _enclosing;
        //        this.id = id;
        //    }

        //    public override void Run()
        //    {
        //        try
        //        {
        //            base.Run();
        //        }
        //        catch (OutOfMemoryException e)
        //        {
        //            OutOfMemoryErrorDispatcher.OnOutOfMemory(e);
        //        }
        //        finally
        //        {
        //            try
        //            {
        //                this._enclosing.idQ.Offer(this.id);
        //            }
        //            catch
        //            {
        //            }
        //        }
        //    }

        //    private readonly PoolExecutorThreadFactory _enclosing;
        //}
	}
}
