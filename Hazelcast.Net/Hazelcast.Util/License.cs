using System;
using System.Collections.Generic;

namespace Hazelcast.Util
{
    internal class LicenseExtractor
    {
        public static char[] chars = "dsx4MZQvo2tqegGLpWhCDXnPYzciBR3murF5SA7KIObaw1Vf6JNyjEl0UTkH".ToCharArray();
        public static char[] digits = "0123456789".ToCharArray();
        public static int length = chars.Length;
        public static int reserved = 25;
        public static int yearBase = 2000;
        public static string INF_STR_ENTERPRISE = "HazelcastEnterprise";
        public static string INF_STR_MANCENTER = "ManagementCenter";
        public static string INF_STR_SECURITY = "SecurityOnlyEnterprise";

        public static License ExtractLicense(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey))
            {
                throw new ArgumentException("License key can not be empty.");
            }

            var keyTokens = licenseKey.Split('#');

            var originalKey = keyTokens[keyTokens.Length - 1].ToCharArray();
            if (length != originalKey.Length)
            {
                throw new InvalidLicenseException("Invalid License Key!");
            }

            var key = new char[length];
            Array.Copy(originalKey, key, length);
            var fp = key[reserved - 1];
            key[reserved - 1] = (char) 0;
            var lp = key[reserved];
            key[reserved] = (char) 0;

            char[] hash = CalculateHash(key);
            if (hash[0] != fp || hash[hash.Length - 1] != lp)
            {
                throw new InvalidLicenseException("Invalid License Key!");
            }

            var ix = 0;
            var r = key[ix++];
            var isTrial = key[ix0(r)] == '1';

            var t = key[ix++];
            var type = key[ix0(t)];
            LicenseType licenseType ;
            switch (type)
            {
                case '0':
                    licenseType = LicenseType.MANAGEMENT_CENTER;
                    break;
                case '1':
                    licenseType = LicenseType.ENTERPRISE;
                    break;
                case '2':
                    licenseType = LicenseType.ENTERPRISE_SECURITY_ONLY;
                    break;
                default:
                    licenseType = default(LicenseType);
                    break;
            }
            var d0 = key[ix++];
            var d1 = key[ix++];
            var day = ix1(key[ix0(d0)])*10 + ix1(key[ix0(d1)]);

            var m0 = key[ix++];
            var m1 = key[ix++];
            var month = ix1(key[ix0(m0)])*10 + ix1(key[ix0(m1)]);

            var y0 = key[ix++];
            var y1 = key[ix++];
            var year = yearBase + ix1(key[ix0(y0)])*10 + ix1(key[ix0(y1)]);


            var cal = new DateTime(year, month, day);

            var n0 = key[ix++];
            var n1 = key[ix++];
            var n2 = key[ix++];
            var n3 = key[ix++];
            var nodes = ix1(key[ix0(n0)])*1000 + ix1(key[ix0(n1)])*100
                        + ix1(key[ix0(n2)])*10 + ix1(key[ix0(n3)]);

            var c0 = key[ix++];
            var c1 = key[ix++];
            var c2 = key[ix++];
            var c3 = key[ix++];
            var clients = ix1(key[ix0(c0)])*1000 + ix1(key[ix0(c1)])*100
                          + ix1(key[ix0(c2)])*10 + ix1(key[ix0(c3)]);

            var h0 = key[ix++];
            var h1 = key[ix++];
            var h2 = key[ix++];
            var h3 = key[ix++];
            var h4 = key[ix++];
            var h5 = key[ix++];
            var h6 = key[ix++];
            var h7 = key[ix++];

            var hdAmount = ix1(key[ix0(h0)])*1000000 + ix1(key[ix0(h1)])*1000000 + ix1(key[ix0(h2)])*100000 +
                           ix1(key[ix0(h3)])*10000 + ix1(key[ix0(h4)])*1000
                           + ix1(key[ix0(h5)])*100 + ix1(key[ix0(h6)])*10 + ix1(key[ix0(h7)]);
            return new License(0, licenseKey, null, cal, isTrial, null, null, nodes, clients, hdAmount, licenseType);
        }

        private static int ix0(char c)
        {
            return ix(chars, c);
        }

        private static int ix1(char c)
        {
            return ix(digits, c);
        }

        private static int ix(char[] cc, char c)
        {
            for (var i = 0; i < cc.Length; i++)
            {
                if (c == cc[i])
                {
                    return i;
                }
            }
            return -1;
        }

        public static char[] CalculateHash(char[] a)
        {
            if (a == null)
            {
                return new[] {'0'};
            }
            var result = 1;
            foreach (var  element in a)
            {
                result = 31*result + element;
            }
            return Math.Abs(result).ToString().ToCharArray();
        }

        public static LicenseVersion ExtractLicenseVersion(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length > 2)
                {
                    var versionPart1 = int.Parse(parts[0]);
                    var versionPart2 = int.Parse(parts[1]);
                    if (versionPart1 >= 3 && versionPart2 >= 5)
                        return LicenseVersion.V2;
                    return LicenseVersion.V1;
                }
                else
                {
                    var rcParts = parts[1].Split('-');
                    var versionPart1 = int.Parse(parts[0]);
                    var versionPart2 = int.Parse(rcParts[0]);
                    if (versionPart1 >= 3 && versionPart2 >= 5)
                        return LicenseVersion.V2;
                    return LicenseVersion.V1;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return LicenseVersion.V1;
            }
        }

        public static bool IsExpired(License license)
        {
            return DateTime.Now.CompareTo(license.expiryDate) > 0;
        }

        public static License CheckLicenseKey(string licenseKey, List<LicenseType> requiredLicenseTypes)
        {
            if (licenseKey == null)
            {
                throw new InvalidLicenseException("License Key not configured!");
            }

            var license = ExtractLicense(licenseKey);

            if (license.trial && IsExpired(license))
            {
                throw new InvalidLicenseException("Trial license has expired! Please contact sales@hazelcast.com");
            }

            if (IsExpired(license))
            {
                throw new InvalidLicenseException("Enterprise License has expired! Please contact sales@hazelcast.com");
            }

            if (!requiredLicenseTypes.Contains(license.type))
            {
                throw new InvalidLicenseException("Invalid License Type! Please contact sales@hazelcast.com");
            }

            return license;
        }
    }

    internal class License
    {
        public static int UNLIMITED_MARKER = 9999;
        public static int UNLIMITED_MARKER_HD_CACHE = 99999999;
        internal int allowedNativeMemorySize;
        internal int allowedNumberOfClients;
        internal int allowedNumberOfNodes;
        internal string companyName;
        internal DateTime creationDate;
        internal string creatorEmail;
        internal string email;
        internal DateTime expiryDate;
        internal string key;
        internal LicenseChannel licenseChannel;
        internal long licenseId;
        internal string pardotId;
        internal bool trial;
        internal LicenseType type;
        internal LicenseVersion version;

        internal License(int licenseId, string key,
            string companyName, DateTime expiryDate, bool trial, string email, string pardotId,
            int allowedNumberOfNodes, int allowedNumberOfClients, int allowedNativeMemorySize, LicenseType type)
        {
            this.allowedNativeMemorySize = allowedNativeMemorySize;
            this.allowedNumberOfClients = allowedNumberOfClients;
            this.allowedNumberOfNodes = allowedNumberOfNodes;
            this.companyName = companyName;
            this.email = email;
            this.expiryDate = expiryDate;
            this.key = key;
            this.licenseId = licenseId;
            this.pardotId = pardotId;
            this.trial = trial;
            this.type = type;
        }
    }

    internal enum LicenseVersion
    {
        V1 = 0,
        V2 = 1
    }

    //internal class LicenseVersionHelper
    //{
    //    internal static LicenseVersion GetDefault()
    //    {
    //        return LicenseVersion.V1;
    //    }

    //    internal static string ToString(LicenseVersion input)
    //    {
    //        switch (input)
    //        {
    //            case LicenseVersion.V1:
    //                return "V1-pre 3.5";
    //            case LicenseVersion.V2:
    //                return "V2 3.5+";
    //        }
    //        return null;
    //    }

    //    internal static LicenseVersion GetLicenseVersion(int code)
    //    {
    //        return (LicenseVersion) code;
    //    }
    //}

    internal enum LicenseType
    {
        MANAGEMENT_CENTER = 1, // "Management Center"),
        ENTERPRISE = 0, // "Enterprise"),
        ENTERPRISE_SECURITY_ONLY = 2 // "Enterprise only with security");
    }

    public class InvalidLicenseException : Exception
    {
        public InvalidLicenseException(string s) : base(s)
        {
        }
    }

    public enum LicenseChannel
    {
        LICENSE_GENERATOR = 0, // "Web"),
        WEB = 1 // "Dowload");
    }
}