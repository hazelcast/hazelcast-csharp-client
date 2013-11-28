using Hazelcast.Net.Ext;


namespace Hazelcast.Util.Executor
{
    //public abstract class AbstractExecutorThreadFactory : ThreadFactory
    //{
    //    protected internal readonly ThreadGroup threadGroup;

    //    public AbstractExecutorThreadFactory(ThreadGroup threadGroup)
    //    {
    //        this.threadGroup = threadGroup;
    //    }

    //    public Hazelcast.Net.Ext.Thread NewThread(Runnable r)
    //    {
    //        Hazelcast.Net.Ext.Thread t = CreateThread(r);
    //        if (t.IsDaemon())
    //        {
    //            t.SetDaemon(false);
    //        }
    //        if (t.GetPriority() != Hazelcast.Net.Ext.Thread.NormPriority)
    //        {
    //            t.SetPriority(Hazelcast.Net.Ext.Thread.NormPriority);
    //        }
    //        return t;
    //    }

    //    protected internal abstract Hazelcast.Net.Ext.Thread CreateThread(Runnable r);
    //}
}
