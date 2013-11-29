using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Logging;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;

namespace Hazelcast.IO.Serialization
{
    internal sealed class PortableHookLoader
    {
        private const string FactoryId = "com.hazelcast.IPortableHook";

        private readonly IDictionary<int, IPortableFactory> configuredFactories;

        private readonly IDictionary<int, IPortableFactory> factories = new Dictionary<int, IPortableFactory>();
        private ICollection<IClassDefinition> definitions = new HashSet<IClassDefinition>();

        internal PortableHookLoader(IDictionary<int, IPortableFactory> configuredFactories)
        {
            this.configuredFactories = configuredFactories;
            Load();
        }

        private void Load()
        {
            try
            {
                IPortableHook[] hooks =
                {
                    new SpiPortableHook(), new PartitionPortableHook(), new ClientPortableHook(),
                    new ClientTxnPortableHook(), new MapPortableHook(), new QueuePortableHook(),
                    new MultiMapPortableHook(), new CollectionPortableHook(), new ExecutorPortableHook(),
                    new TopicPortableHook(), new LockPortableHook(), new SemaphorePortableHook(),
                    new AtomicLongPortableHook(), new CountDownLatchPortableHook()
                };
                foreach (IPortableHook hook in hooks)
                {
                    IPortableFactory factory = hook.CreateFactory();
                    if (factory != null)
                    {
                        Register(hook.GetFactoryId(), factory);
                    }
                    ICollection<IClassDefinition> defs = hook.GetBuiltinDefinitions();
                    if (defs != null && defs.Count > 0)
                    {
                        definitions = definitions.Union(defs).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            if (configuredFactories != null)
            {
                foreach (var entry in configuredFactories)
                {
                    Register(entry.Key, entry.Value);
                }
            }
        }

        internal IDictionary<int, IPortableFactory> GetFactories()
        {
            return factories;
        }

        internal ICollection<IClassDefinition> GetDefinitions()
        {
            return definitions;
        }

        private void Register(int factoryId, IPortableFactory factory)
        {
            IPortableFactory current;
            factories.TryGetValue(factoryId, out current);
            if (current != null)
            {
                if (current.Equals(factory))
                {
                    Logger.GetLogger(GetType())
                        .Warning("IPortableFactory[" + factoryId + "] is already registered! Skipping " + factory);
                }
                else
                {
                    throw new ArgumentException("IPortableFactory[" + factoryId + "] is already registered! " + current +
                                                " -> " + factory);
                }
            }
            else
            {
                factories.Add(factoryId, factory);
            }
        }
    }
}