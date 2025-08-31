using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFR
{
    public class Configuration
    {
        public const string configSource = "GTFR.cfg";

        public Configuration ()
        {
            var configPath = Path.Combine (Paths.ConfigPath, configSource);
            var configFile = new ConfigFile (configPath, true);
        }
    }
}
