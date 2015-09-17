namespace Hazelcast.Core
{
    /// <summary>Lifecycle event fired when IHazelcastInstance's state changes.</summary>
    /// <remarks>
    ///     Lifecycle event fired when IHazelcastInstance's state changes.
    ///     Events are fired when instance:
    ///     <ul>
    ///         <li>Starting</li>
    ///         <li>Started</li>
    ///         <li>Shutting down</li>
    ///         <li>Shut down completed</li>
    ///         <li>Merging</li>
    ///         <li>Merged</li>
    ///     </ul>
    /// </remarks>
    /// <seealso cref="ILifecycleListener">ILifecycleListener</seealso>
    /// <seealso cref="IHazelcastInstance.GetLifecycleService()">IHazelcastInstance.GetLifecycleService()</seealso>
    public sealed class LifecycleEvent
    {
        /// <summary>lifecycle states</summary>
        public enum LifecycleState
        {
            Starting,
            Started,
            ShuttingDown,
            Shutdown,
            Merging,
            Merged,
            ClientConnected,
            ClientDisconnected
        }

        private readonly LifecycleState state;

        public LifecycleEvent(LifecycleState state)
        {
            this.state = state;
        }

        public LifecycleState GetState()
        {
            return state;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (!(o is LifecycleEvent))
            {
                return false;
            }
            var that = (LifecycleEvent) o;
            if (state != that.state)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return state.GetHashCode();
        }

        public override string ToString()
        {
            return "LifecycleEvent [state=" + state + "]";
        }
    }
}