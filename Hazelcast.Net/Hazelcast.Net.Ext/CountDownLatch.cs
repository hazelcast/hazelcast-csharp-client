namespace Hazelcast.Net.Ext
{
    //using System;
    //using System.Threading;

    //public class CountDownLatch
    //{
    //    private int _count;
    //    private readonly ManualResetEvent _done = new ManualResetEvent (false);

    //    public CountDownLatch (int count)
    //    {
    //        this._count = count;
    //        if (count == 0) {
    //            _done.Set ();
    //        }
    //    }

    //    public void Await ()
    //    {
    //        _done.WaitOne ();
    //    }

    //    public bool Await (long timeout, TimeUnit unit)
    //    {
    //        return _done.WaitOne ((int) unit.Convert (timeout, TimeUnit.MILLISECONDS));
    //    }

    //    public void CountDown ()
    //    {
    //        if (Interlocked.Decrement (ref _count) == 0) {
    //            _done.Set ();
    //        }
    //    }
    //}
}
