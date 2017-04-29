using System.Configuration;
using System.Xml;

namespace GrowCreate.PipelineCRM.Config
{
    public class DigestRowElement : ConfigurationElement
    {
        public string InnerHtml { get; private set; }
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            InnerHtml = reader.ReadElementContentAsString();
        }
    }
}