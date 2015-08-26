using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Logging;

namespace Hazelcast.Util
{
    class EnvironmentUtil
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (EnvironmentUtil));

        public static int ReadEnvironmentVar(string var)
        {
            var p = 0;
            var param = Environment.GetEnvironmentVariable(var);
            try
            {
                if (param != null)
                {
                    p = Convert.ToInt32(param, 10);
                }
            }
            catch (Exception)
            {
                Logger.Warning("Provided value is not a valid value : " + param);
            }
            return p;
        }
    }
}
