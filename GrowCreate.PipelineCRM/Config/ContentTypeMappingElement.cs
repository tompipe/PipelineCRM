using System.Configuration;

namespace GrowCreate.PipelineCRM.Config
{
    public class ContentTypeMappingElement : ConfigurationElement
    {
        [ConfigurationProperty(nameof(Type), IsRequired = true, IsKey = true)]
        public string Type
        {
            get
            {
                return this[nameof(Type)] as string;
            }
            set
            {
                this[nameof(Type)] = value;
            }
        }

        [ConfigurationProperty(nameof(Alias), IsRequired = true)]
        public string Alias
        {
            get
            {
                return this[nameof(Alias)] as string;
            }
            set
            {
                this[nameof(Alias)] = value;
            }
        }
    }
}