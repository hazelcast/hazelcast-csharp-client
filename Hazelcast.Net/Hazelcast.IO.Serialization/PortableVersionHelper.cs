using System;

namespace Hazelcast.IO.Serialization
{
	internal sealed class PortableVersionHelper
	{
		public static int GetVersion(IPortable portable, int defaultVersion)
		{
			int version = defaultVersion;
			if (portable is IVersionedPortable)
			{
				IVersionedPortable versionedPortable = (IVersionedPortable)portable;
				version = versionedPortable.GetClassVersion();
				if (version < 0)
				{
					throw new ArgumentException("Version cannot be negative!");
				}
			}
			return version;
		}
	}
}
