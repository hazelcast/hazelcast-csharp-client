namespace Hazelcast.Client.Test
{
	[System.Serializable]
	public class GenericEvent
	{
		internal readonly string value;

		public GenericEvent(string value)
		{
			this.value = value;
		}

		public virtual string GetValue()
		{
			return value;
		}
	}
}
