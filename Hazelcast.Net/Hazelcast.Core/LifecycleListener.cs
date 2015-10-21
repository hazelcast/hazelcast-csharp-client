using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelcast.Core
{
    public class LifecycleListener : ILifecycleListener
    {
        private readonly Action<LifecycleEvent> _action;

        public LifecycleListener(Action<LifecycleEvent> action)
        {
            _action = action;
        }

        public void StateChanged(LifecycleEvent lifecycleEvent)
        {
            _action(lifecycleEvent);
        }
    }
}
