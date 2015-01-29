using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelcast.Core
{
    public class MapEvent : AbstractMapEvent
    {
        private readonly int numberofEntriesAffected ;

        public MapEvent(object source, IMember member, EntryEventType eventType, int numberofEntriesAffected) : base(source, member, eventType)
        {
            this.numberofEntriesAffected = numberofEntriesAffected;
        }

        /// <summary>Returns the number of entries affected by this event.</summary>
        /// <returns>the number of entries affected</returns>
        public int GetNumberOfEntriesAffected()
        {
            return numberofEntriesAffected;
        }
    }
}
