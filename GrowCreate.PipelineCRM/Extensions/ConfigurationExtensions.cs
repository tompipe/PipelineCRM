using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class ConfigurationExtensions
    {
        internal static IContentType GetMappedContentType(Type t)
        {
            var mappings = PipelineConfig.GetConfig().AppSettings.ContentTypeMappings;
            
            var element = mappings[t];

            if (element != null)
            {
                // TODO: cache
                return ApplicationContext.Current.Services.ContentTypeService.GetContentType(element.Alias);
            }

            return null;
        }

        internal static IContentType GetMappedContentType<T>() where T : ExtendableEntityBase
        {
            return GetMappedContentType(typeof(T));
        }

        internal static IContentType GetMappedContentType<T>(this T entity) where T : ExtendableEntityBase
        {
            return GetMappedContentType(entity.GetType());
        }
    }
}