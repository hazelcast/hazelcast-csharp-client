namespace Hazelcast.Core
{
    internal sealed class TerminatedLifecycleService : ILifecycleService
    {
        public bool IsRunning()
        {
            return false;
        }

        public void Shutdown()
        {
        }

        public void Terminate()
        {
        }

        public string AddLifecycleListener(ILifecycleListener lifecycleListener)
        {
            throw new HazelcastInstanceNotActiveException();
        }

        public bool RemoveLifecycleListener(string registrationId)
        {
            throw new HazelcastInstanceNotActiveException();
        }
    }
}