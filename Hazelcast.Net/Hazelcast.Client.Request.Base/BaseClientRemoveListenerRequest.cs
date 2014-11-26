using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Base
{
	internal abstract class BaseClientRemoveListenerRequest : ClientRequest
	{
		protected internal string name;

		protected internal string registrationId;
		protected internal BaseClientRemoveListenerRequest(string name, string registrationId
			)
		{
			this.name = name;
			this.registrationId = registrationId;
		}

		public virtual string GetRegistrationId()
		{
			return registrationId;
		}

		public virtual void SetRegistrationId(string registrationId)
		{
			this.registrationId = registrationId;
		}

		public virtual string GetName()
		{
			return name;
		}

		public virtual void SetName(string name)
		{
			this.name = name;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void Write(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteUTF("r", registrationId);
		}
	}
}
