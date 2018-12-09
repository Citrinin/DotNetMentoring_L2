using System.Configuration;

namespace ImageWatcher.Configuration
{
    public class ImageWatcherConfigurationSection: ConfigurationSection
    {
        [ConfigurationProperty("imagesWatchers")]
        public ImagesWatcherElementCollection ImagesWatchers => (ImagesWatcherElementCollection)this["imagesWatchers"];
    }
}
