# CP SubSystem

> [!WARNING]
> The CP SubSystem operates in the unsafe mode by default without the strong consistency guarantee. See the CP Subsystem Unsafe Mode section for more information. You should set a positive number to the CP member count configuration to enable CP Subsystem and use it with the strong consistency guarantee. See the CP Subsystem Configuration section for details.

> [!NOTE]
> See the original Java client [CP SubSystem](https://docs.hazelcast.com/imdg/latest/cp-subsystem/cp-subsystem.html) documentation for more details.

The CP SubSystem is a component of a Hazelcast cluster that builds a strongly consistent layer for a set of distributed data structures. Its APIs can be used for implementing distributed coordination use cases, such as leader election, distributed locking, synchronization, and metadata management. It is accessed via the `IHazelcastClient.CPSubSystem` property. Its data structures are CP with respect to the [CAP principle](http://awoc.wolski.fi/dlib/big-data/Brewer_podc_keynote_2000.pdf), i.e., they always maintain [linearizability](https://aphyr.com/posts/313-strong-consistency-models) and prefer consistency over availability during network partitions. Besides network partitions, the CP SubSystem withstands server and client failures.

Currently, the C# client CP SubSystem implements the following services:
* [AtomicLong](distributed-objects/atomiclong.md)
* [AtomicRef](distributed-objects/atomicref.md)
* [FencedLock](distributed-objects/fencedlock.md)
* [CPMap](distributed-objects/cpmap.md)
* [CountDownLatch](distributed-objects/countdownlatch.md)