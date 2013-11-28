using System;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util.Executor;


namespace Hazelcast.Util.Executor
{
	
	public sealed class SingleExecutorThreadFactory //: AbstractExecutorThreadFactory
	{
    //    private readonly string threadName;

    //    public SingleExecutorThreadFactory(ThreadGroup threadGroup, string threadName) : base(threadGroup)
    //    {
    //        this.threadName = threadName;
    //    }

    //    protected internal override Hazelcast.Net.Ext.Thread CreateThread(Runnable r)
    //    {
    //        return new SingleExecutorThreadFactory.ManagedThread(this, r);
    //    }

    //    private class ManagedThread : Hazelcast.Net.Ext.Thread
    //    {
    //        public ManagedThread(SingleExecutorThreadFactory _enclosing, Runnable target) : base(this._enclosing.threadGroup, target, this._enclosing.threadName)
    //        {
    //            this._enclosing = _enclosing;
    //        }

    //        public override void Run()
    //        {
    //            try
    //            {
    //                base.Run();
    //            }
    //            catch (OutOfMemoryException e)
    //            {
    //                OutOfMemoryErrorDispatcher.OnOutOfMemory(e);
    //            }
    //        }

    //        private readonly SingleExecutorThreadFactory _enclosing;
    //    }
    }
}
