using System;
using Hazelcast.Eventing;

namespace Hazelcast.Tests
{
    class EventExperiment
    {
        // current:

        Guid AddMembershipListener(object listner) { return Guid.Empty; }
        void RemoveMembershipListener(Guid id) { }

        // better:

        private IEventHandlers<MembershipEvent> MembershipHandlers { get; } = new EventHandlers<MembershipEvent>();

        // but:
        //
        // should we keep using these listeners,
        // or go with c# events and multicast delegates?

        public class MembershipEvent { }

        public class MembershipHandler : IEventHandler<MembershipEvent>
        {
            public void Handle(MembershipEvent e)
            {

            }
        }

        public void Sample()
        {
            var thing = new EventExperiment();
            var id = thing.MembershipHandlers.Add(new MembershipHandler());
            thing.MembershipHandlers.Remove(id);

            var e = new MembershipEvent();
            thing.MembershipHandlers.Handle(e);
        }
    }
}
