using System;
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
	/// <summary>This is an exception thrown when an exception occurs while serializing/deserializing objects.
	/// 	</summary>
	/// <remarks>This is an exception thrown when an exception occurs while serializing/deserializing objects.
	/// 	</remarks>
	[System.Serializable]
	public class HazelcastSerializationException : HazelcastException
	{
		public HazelcastSerializationException(string message) : base(message)
		{
		}

		public HazelcastSerializationException(string message, Exception cause) : base(message
			, cause)
		{
		}

		public HazelcastSerializationException(Exception e) : base(e)
		{
		}
	}
}
