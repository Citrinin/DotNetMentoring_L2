using System.Configuration;

namespace ImageWatcher.Configuration
{
    public class ImagesWatcherElement : ConfigurationElement
    {
        [ConfigurationProperty("inFolder")]
        public string InFolder => (string) this["inFolder"];

        [ConfigurationProperty("outFolder")]
        public string OutFolder => (string) this["outFolder"];

        [ConfigurationProperty("corruptedFolder")]
        public string CorruptedFolder => (string)this["corruptedFolder"];

        [ConfigurationProperty("prefix")]
        public string Prefix => (string) this["prefix"];
    }
}