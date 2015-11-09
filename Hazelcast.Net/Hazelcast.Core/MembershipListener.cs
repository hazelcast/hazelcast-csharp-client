using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hazelcast.Core
{
    public class MembershipListener : IMembershipListener
    {
        public Action<MembershipEvent> OnMemberAdded { get; set; }
        public Action<MembershipEvent> OnMemberRemoved { get; set; }
        public Action<MemberAttributeEvent> OnMemberAttributeChanged { get; set; }

        public void MemberAdded(MembershipEvent membershipEvent)
        {
            if (OnMemberAdded != null) OnMemberAdded(membershipEvent);
        }

        public void MemberRemoved(MembershipEvent membershipEvent)
        {
            if (OnMemberRemoved != null) OnMemberRemoved(membershipEvent);
        }

        public void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent)
        {
            if (OnMemberAttributeChanged != null)
                OnMemberAttributeChanged(memberAttributeEvent);
        }
    }
}
