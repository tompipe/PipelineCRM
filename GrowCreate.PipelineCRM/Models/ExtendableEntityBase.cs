using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Validation;
using GrowCreate.PipelineCRM.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Our.Umbraco.NestedContent.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace GrowCreate.PipelineCRM.Models
{
    public abstract class ExtendableEntityBase
    {
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CustomProps
        {
            get; set;
        }

        [Ignore]
        [JsonIgnore]
        public IPublishedContent CustomProperties
        {
            get; set;
        }
    }
}