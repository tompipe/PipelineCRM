using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GrowCreate.PipelineCRM.Config
{
    public class PipelineConfig : ConfigurationSection
    {
        public static PipelineConfig GetConfig()
        {
            return ConfigurationManager.GetSection("PipelineConfig") as PipelineConfig;
        }

        [ConfigurationProperty("appSettings")]
        public PipelineOptions AppSettings 
        { 
            get 
            {
                return (PipelineOptions)this["appSettings"]; 
            } 
            set 
            {
                this["appSettings"] = value; 
            } 
        }

        [ConfigurationProperty("digestBody")]
        public DigestBodyElement DigestBody
        {
            get
            {
                return (DigestBodyElement)this["digestBody"];
            }
            set
            {
                this["digestBody"] = value;
            }
        }

        [ConfigurationProperty("digestRow")]
        public DigestRowElement DigestRow
        {
            get
            {
                return (DigestRowElement)this["digestRow"];
            }
            set
            {
                this["digestRow"] = value;
            }
        }

    }
}