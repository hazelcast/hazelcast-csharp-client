using System;
using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the core options.
    /// </summary>
    public class CoreOptions
    {
        /// <summary>
        /// Gets the clock options.
        /// </summary>
        public ClockOptions Clock { get; private set; } = new ClockOptions();

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal CoreOptions Clone()
        {
            return new CoreOptions
            {
                Clock = Clock.Clone()
            };
        }
    }
}
