using System.Configuration;

namespace ImageWatcher.Configuration
{
    [ConfigurationCollection(typeof(ImagesWatcherElement), AddItemName = "imagesWatcher")]
    public class ImagesWatcherElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ImagesWatcherElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ImagesWatcherElement)element).InFolder;
        }
    }
}