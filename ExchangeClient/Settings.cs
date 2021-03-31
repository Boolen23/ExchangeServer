using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExchangeClient
{
    [Serializable]
    public class Settings
    {
        public string BroadcastGroup { get; set; } = "239.255.255.250";
        public int ReciveDelayMs { get; set; } = 0;
        public static Settings Load()
        {
            if (File.Exists("settings.xml"))
            {
                Settings s = null;
                using (var stream = System.IO.File.OpenRead("settings.xml"))
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    s = serializer.Deserialize(stream) as Settings;
                }
                return s;
            }
            else
            {
                Settings s = new Settings();
                using (var writer = new StreamWriter("settings.xml"))
                {
                    var serializer = new XmlSerializer(s.GetType());
                    serializer.Serialize(writer, s);
                    writer.Flush();
                }
                return s;
            }
        }
    }
}
