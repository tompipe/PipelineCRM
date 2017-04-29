using System.Configuration;

namespace GrowCreate.PipelineCRM.Config
{
    public class PipelineOptions : ConfigurationElement
    {
                
        [ConfigurationProperty("createMembers", DefaultValue = true, IsRequired = true)]
        public bool CreateMembers
        {
            get
            {
                return this["createMembers"] is bool && (bool)this["createMembers"];
            }
        }

        [ConfigurationProperty("memberType", IsRequired = true)]
        public string MemberType
        {
            get
            {
                return (string)this["memberType"];
            }
        }

        [ConfigurationProperty("useBoard", DefaultValue = true, IsRequired = true)]
        public bool UseBoard
        {
            get
            {
                return this["useBoard"] is bool && (bool)this["useBoard"];
            }
        }

        [ConfigurationProperty("pageSize", IsRequired = true, DefaultValue = 50)]
        public int PageSize
        {
            get
            {
                return (int)this["pageSize"];
            }
        }

        [ConfigurationProperty("digestTime", IsRequired = true, DefaultValue = 7)]
        public int DigestTime
        {
            get
            {
                return (int)this["digestTime"];
            }
        }

        [ConfigurationProperty("digestSubject", IsRequired = true, DefaultValue = "Pipeline digest")]
        public string DigestSubject
        {
            get
            {
                return (string)this["digestSubject"];
            }
        }

        [ConfigurationProperty("digestSender", IsRequired = true, DefaultValue = "no-reply@website.com")]
        public string DigestSender
        {
            get
            {
                return (string)this["digestSender"];
            }
        }     
        
        [ConfigurationProperty(nameof(ContentTypeMappings))]
        public ContentTypeMappingElementCollection ContentTypeMappings
        {
            get
            {
                return this[nameof(ContentTypeMappings)] as ContentTypeMappingElementCollection;
            }
        }
    }
}