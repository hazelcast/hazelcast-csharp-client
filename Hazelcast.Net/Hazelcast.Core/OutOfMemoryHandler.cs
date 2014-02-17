using System;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Handler for <code>OutOfMemoryError</code>
    ///     <p>
    ///         When an <code>OutOfMemoryError</code> is caught by Hazelcast threads,
    ///         <code>OutOfMemoryHandler</code> is called for ALL <code>IHazelcastInstance</code>s
    ///         knows by current JVM (actually ClassLoader).
    /// </summary>
    /// <remarks>
    ///     Handler for <code>OutOfMemoryError</code>
    ///     <p>
    ///         When an <code>OutOfMemoryError</code> is caught by Hazelcast threads,
    ///         <code>OutOfMemoryHandler</code> is called for ALL <code>IHazelcastInstance</code>s
    ///         knows by current JVM (actually ClassLoader).
    ///     </p>
    ///     <p>
    ///         <b>Warning: </b> <tt>OutOfMemoryHandler</tt> may not be called although JVM throws
    ///         <tt>OutOfMemoryError</tt>.
    ///         Because error may be thrown from an external (user thread) thread
    ///         and Hazelcast may not be informed about <tt>OutOfMemoryError</tt>.
    ///     </p>
    /// </remarks>
    /// <seealso cref="System.OutOfMemoryException">System.OutOfMemoryException</seealso>
    /// <seealso cref="Hazelcast#setOutOfMemoryHandler(com.hazelcast.core.OutOfMemoryHandler)">Hazelcast#setOutOfMemoryHandler(com.hazelcast.core.OutOfMemoryHandler)</seealso>
    internal abstract class OutOfMemoryHandler
    {
        /// <summary>
        ///     When an <code>OutOfMemoryError</code> is caught by Hazelcast threads,
        ///     this method is called for ALL <code>IHazelcastInstance</code>s
        ///     knows by current JVM (actually ClassLoader).
        /// </summary>
        /// <remarks>
        ///     When an <code>OutOfMemoryError</code> is caught by Hazelcast threads,
        ///     this method is called for ALL <code>IHazelcastInstance</code>s
        ///     knows by current JVM (actually ClassLoader).
        ///     <p>
        ///         User can shutdown <tt>IHazelcastInstance</tt>, call <code>System.exit()</code>,
        ///         just log the error etc.
        ///         Default handler tries to close socket connections to other nodes and shutdown
        ///         <tt>IHazelcastInstance</tt>.
        ///     </p>
        ///     <p>
        ///         <b>Warning: </b> <tt>OutOfMemoryHandler</tt> may not be called although JVM throws
        ///         <tt>OutOfMemoryError</tt>.
        ///         Because error may be thrown from an external (user thread) thread
        ///         and Hazelcast may not be informed about <tt>OutOfMemoryError</tt>.
        ///     </p>
        /// </remarks>
        /// <seealso cref="Inactivate(IHazelcastInstance)">Inactivate(IHazelcastInstance)</seealso>
        /// <seealso cref="TryCloseConnections(IHazelcastInstance)">TryCloseConnections(IHazelcastInstance)</seealso>
        /// <seealso cref="TryStopThreads(IHazelcastInstance)">TryStopThreads(IHazelcastInstance)</seealso>
        /// <seealso cref="TryShutdown(IHazelcastInstance)">TryShutdown(IHazelcastInstance)</seealso>
        /// <param name="oom">OutOfMemoryError thrown by JVM</param>
        /// <param name="hazelcastInstances">
        ///     All HazelcastInstances known by JVM,
        ///     can include inactive or NULL instances.
        /// </param>
        public abstract void OnOutOfMemory(OutOfMemoryException oom, IHazelcastInstance[] hazelcastInstances);

        /// <summary>Just inactivates <tt>IHazelcastInstance</tt>; leaves threads, connections untouched.</summary>
        /// <remarks>Just inactivates <tt>IHazelcastInstance</tt>; leaves threads, connections untouched.</remarks>
        /// <param name="hazelcastInstance"></param>
        protected internal void Inactivate(IHazelcastInstance hazelcastInstance)
        {
            OutOfMemoryErrorDispatcher.Helper.Inactivate(hazelcastInstance);
        }

        /// <summary>Tries to close server socket and connections to other <tt>IHazelcastInstance</tt>s.</summary>
        /// <remarks>Tries to close server socket and connections to other <tt>IHazelcastInstance</tt>s.</remarks>
        /// <param name="hazelcastInstance"></param>
        protected internal void TryCloseConnections(IHazelcastInstance hazelcastInstance)
        {
            OutOfMemoryErrorDispatcher.Helper.TryCloseConnections(hazelcastInstance);
        }

        /// <summary>Tries to stop internal Hazelcast threads (such as service thread, IO threads, executor threads).</summary>
        /// <remarks>Tries to stop internal Hazelcast threads (such as service thread, IO threads, executor threads).</remarks>
        /// <param name="hazelcastInstance"></param>
        protected internal void TryStopThreads(IHazelcastInstance hazelcastInstance)
        {
            OutOfMemoryErrorDispatcher.Helper.TryStopThreads(hazelcastInstance);
        }

        /// <summary>
        ///     Tries to shutdown <tt>IHazelcastInstance</tt> forcefully;
        ///     including closing sockets and connections, stopping threads etc.
        /// </summary>
        /// <remarks>
        ///     Tries to shutdown <tt>IHazelcastInstance</tt> forcefully;
        ///     including closing sockets and connections, stopping threads etc.
        /// </remarks>
        /// <param name="hazelcastInstance"></param>
        protected internal void TryShutdown(IHazelcastInstance hazelcastInstance)
        {
            OutOfMemoryErrorDispatcher.Helper.TryShutdown(hazelcastInstance);
        }
    }
}