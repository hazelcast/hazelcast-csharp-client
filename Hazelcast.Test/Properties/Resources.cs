using System;
using System.IO;
using System.Text;

namespace Hazelcast.Test
{
    internal static class Resources
    {
        //Binary files
        public static byte[] client1 {get { return GetBytes("Resources.client1.pfx"); }}
        public static byte[] client2 {get { return GetBytes("Resources.client2.pfx"); }}

        //Text files
        public static string hazelcast_config_full {get { return GetXmlResourceContent("hazelcast-client-full"); }}
        public static string hazelcast {get { return GetXmlResourceContent("hazelcast"); }}
        public static string hazelcast_delay {get { return GetXmlResourceContent("hazelcast-delay"); }}
        public static string hazelcast_hb {get { return GetXmlResourceContent("hazelcast-hb"); }}
        public static string hazelcast_ipv6 {get { return GetXmlResourceContent("hazelcast-ipv6"); }}
        public static string hazelcast_ma_required {get { return GetXmlResourceContent("hazelcast-ma-required"); }}
        public static string hazelcast_ma_optional {get { return GetXmlResourceContent("hazelcast-ma-optional"); }}
        public static string hazelcast_nearcache {get { return GetXmlResourceContent("hazelcast-nearcache"); }}
        public static string hazelcast_ssl_signed {get { return GetXmlResourceContent("hazelcast-ssl-signed"); }}
        public static string hazelcast_ssl {get { return GetXmlResourceContent("hazelcast-ssl"); }}
        public static string hazelcast_stat {get { return GetXmlResourceContent("hazelcast-stat"); }}
        public static string hazelcast_quick_node_switching { get { return GetXmlResourceContent("hazelcast-quick-node-switching"); } }
        public static string hazelcast_lite_member { get { return GetXmlResourceContent("hazelcast-lite-member"); } }

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