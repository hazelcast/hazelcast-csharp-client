using System;
using System.IO;
using System.Text;

namespace Hazelcast.Test
{
    internal static class Resources
    {
        //Binary files
        public static byte[] Client1 => GetBytes("Resources.client1.pfx");
        public static byte[] Client2 => GetBytes("Resources.client2.pfx");

        //Text files
        public static string HazelcastConfigFull => GetXmlResourceContent("hazelcast-client-full");
        public static string Hazelcast => GetXmlResourceContent("hazelcast");
        public static string HazelcastDelay => GetXmlResourceContent("hazelcast-delay");
        public static string HazelcastHb => GetXmlResourceContent("hazelcast-hb");
        public static string HazelcastIpv6 => GetXmlResourceContent("hazelcast-ipv6");
        public static string HazelcastMaRequired => GetXmlResourceContent("hazelcast-ma-required");
        public static string HazelcastMaOptional => GetXmlResourceContent("hazelcast-ma-optional");
        public static string HazelcastNearCache => GetXmlResourceContent("hazelcast-nearcache");
        public static string HazelcastSslSigned => GetXmlResourceContent("hazelcast-ssl-signed");
        public static string HazelcastSsl => GetXmlResourceContent("hazelcast-ssl");
        public static string HazelcastStat => GetXmlResourceContent("hazelcast-stat");
        public static string HazelcastCrdtReplication => GetXmlResourceContent("hazelcast-crdt-replication");
        public static string HazelcastLiteMember => GetXmlResourceContent("hazelcast-lite-member");
        public static string hazelcast_kerberos { get { return GetXmlResourceContent("hazelcast-kerberos"); } }

        private static byte[] GetBytes(string name)
        {
            var assembly = typeof(Resources).Assembly;
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    return null;
                }
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        private static string GetXmlResourceContent(string name)
        {
            var fullName = "Resources." + name + ".xml";
            var bytes = GetBytes(fullName);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}