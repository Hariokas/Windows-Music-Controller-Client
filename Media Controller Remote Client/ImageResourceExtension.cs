using System.Reflection;

namespace Media_Controller_Remote_Client;

[ContentProperty("Source")]
public class ImageResourceExtension : IMarkupExtension<ImageSource>
{
    public string Source { set; get; }

    public ImageSource ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Source))
        {
            var lineInfo =
                serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider
                    ? lineInfoProvider.XmlLineInfo
                    : new XmlLineInfo();

            throw new XamlParseException("ImageResourceExtension requires Source property to be set", lineInfo);
        }

        var assemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
        return ImageSource.FromResource(assemblyName + "." + Source,
            typeof(ImageResourceExtension).GetTypeInfo().Assembly);
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return (this as IMarkupExtension<ImageSource>).ProvideValue(serviceProvider);
    }
}