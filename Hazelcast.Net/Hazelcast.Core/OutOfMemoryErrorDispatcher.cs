using System;

namespace Hazelcast.Core
{
    internal static class OutOfMemoryErrorDispatcher
    {
        private static readonly IHazelcastInstance[] instances = new IHazelcastInstance[50];

        private static int size;

        private static OutOfMemoryHandler handler = new DefaultOutOfMemoryHandler();

        //private OutOfMemoryErrorDispatcher()
        //{
        //}

        public static void SetHandler(OutOfMemoryHandler outOfMemoryHandler)
        {
            lock (typeof (OutOfMemoryErrorDispatcher))
            {
                handler = outOfMemoryHandler;
            }
        }

        internal static bool Register(IHazelcastInstance instance)
        {
            lock (typeof (OutOfMemoryErrorDispatcher))
            {
                if (size < instances.Length - 1)
                {
                    instances[size++] = instance;
                    return true;
                }
                return false;
            }
        }

        internal static bool Deregister(IHazelcastInstance instance)
        {
            lock (typeof (OutOfMemoryErrorDispatcher))
            {
                for (int index = 0; index < instances.Length; index++)
                {
                    IHazelcastInstance hz = instances[index];
                    if (hz == instance)
                    {
                        try
                        {
                            int numMoved = size - index - 1;
                            if (numMoved > 0)
                            {
                                Array.Copy(instances, index + 1, instances, index, numMoved);
                            }
                            instances[--size] = null;
                            return true;
                        }
                        catch
                        {
                        }
                    }
                }
                return false;
            }
        }

        internal static void Clear()
        {
            lock (typeof (OutOfMemoryErrorDispatcher))
            {
                for (int i = 0; i < instances.Length; i++)
                {
                    instances[i] = null;
                    size = 0;
                }
            }
        }

        public static void OnOutOfMemory(OutOfMemoryException oom)
        {
            lock (typeof (OutOfMemoryErrorDispatcher))
            {
                if (handler != null)
                {
                    try
                    {
                        handler.OnOutOfMemory(oom, instances);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private class DefaultOutOfMemoryHandler : OutOfMemoryHandler
        {
            public override void OnOutOfMemory(OutOfMemoryException oom, IHazelcastInstance[] hazelcastInstances)
            {
                foreach (IHazelcastInstance instance in hazelcastInstances)
                {
                    if (instance != null)
                    {
                        Helper.TryCloseConnections(instance);
                        Helper.TryStopThreads(instance);
                        Helper.TryShutdown(instance);
                    }
                }
                Console.Error.WriteLine(oom);
            }
        }

        internal sealed class Helper
        {
            public static void TryCloseConnections(IHazelcastInstance hazelcastInstance)
            {
                if (hazelcastInstance == null)
                {
                    return;
                }
                IHazelcastInstance factory = hazelcastInstance;
                CloseSockets(factory);
            }

            private static void CloseSockets(IHazelcastInstance factory)
            {
            }

            //            if (factory.node.connectionManager != null) {
            //                try {
            //                    factory.node.connectionManager.shutdown();
            //                } catch (Throwable ignored) {
            //                }
            //            }
            public static void TryShutdown(IHazelcastInstance hazelcastInstance)
            {
            }

            //            if (hazelcastInstance == null) return;
            //            final HazelcastInstanceImpl factory = (HazelcastInstanceImpl) hazelcastInstance;
            //            closeSockets(factory);
            //            try {
            //                factory.node.shutdown(true, true);
            //            } catch (Throwable ignored) {
            //            }
            public static void Inactivate(IHazelcastInstance hazelcastInstance)
            {
            }

            //            if (hazelcastInstance == null) return;
            //            final HazelcastInstanceImpl factory = (HazelcastInstanceImpl) hazelcastInstance;
            //            factory.node.inactivate();
            public static void TryStopThreads(IHazelcastInstance hazelcastInstance)
            {
            }

            //            if (hazelcastInstance == null) return;
            //            final HazelcastInstanceImpl factory = (HazelcastInstanceImpl) hazelcastInstance;
            //            try {
            //                factory.node.threadGroup.interrupt();
            //            } catch (Throwable ignored) {
            //            }
        }
    }
}