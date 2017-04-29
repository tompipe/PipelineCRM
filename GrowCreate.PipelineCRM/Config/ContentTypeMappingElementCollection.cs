using System;
using System.Configuration;

namespace GrowCreate.PipelineCRM.Config
{
    public class ContentTypeMappingElementCollection : ConfigurationElementCollection
    {
        public ContentTypeMappingElement this[object key]
        {
            get
            {
                return base.BaseGet(key) as ContentTypeMappingElement;
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "map";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            bool isName = false;
            if (!String.IsNullOrEmpty(elementName))
                isName = elementName.Equals(ElementName);
            return isName;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ContentTypeMappingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var typeName = ((ContentTypeMappingElement) element).Type;

            return Type.GetType(typeName);
        }
    }
}