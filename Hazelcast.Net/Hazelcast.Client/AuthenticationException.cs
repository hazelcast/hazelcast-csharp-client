using Hazelcast.Core;


namespace Hazelcast.Client
{
	[System.Serializable]
	public class AuthenticationException : HazelcastException
	{
		public AuthenticationException() : base("Wrong group name and password.")
		{
		}

		public AuthenticationException(string message) : base(message)
		{
		}
	}
}
