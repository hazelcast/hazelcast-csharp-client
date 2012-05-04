using System;
using System.Net;

namespace Hazelcast.Core
{
	public interface Member
	{
		bool isLiteMember();
		
		IPEndPoint getIPEndPoint();
	}
}

