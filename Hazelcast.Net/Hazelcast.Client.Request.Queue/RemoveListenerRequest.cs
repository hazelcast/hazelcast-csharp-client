using Hazelcast.Client.Request.Base;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Queue
{
    /// <summary>Remove listener request belong to the Queue item.</summary>
    /// <remarks>Remove listener request belong to the Queue item.</remarks>
    internal class RemoveListenerRequest : BaseClientRemoveListenerRequest
    {
        protected internal RemoveListenerRequest(string name, string registrationId) : base(name, registrationId)
        {
        }

        public override int GetFactoryId()
        {
            return QueuePortableHook.FId;
        }

        public override int GetClassId()
        {
            return QueuePortableHook.RemoveListener;
        }
    }
}