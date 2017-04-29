using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Models.DetachedPublishedContent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class ExtendableEntityExtensions
    {
        internal static IPublishedContent ParseCustomProps(this ExtendableEntityBase entity)
        {
            if (!string.IsNullOrEmpty(entity.CustomProps))
            {
                try
                {
                    var rawValue = JsonConvert.DeserializeObject<List<JObject>>(entity.CustomProps);

                    var contentType = entity.GetMappedContentType();
                    if (contentType == null)
                    {
                        return null;
                    }

                    var publishedContentType = PublishedContentType.Get(PublishedItemType.Content, contentType.Alias);
                    if (publishedContentType == null)
                    {
                        return null;
                    }

                    var props = publishedContentType.PropertyTypes.Select(propType => new DetachedPublishedProperty(propType, null, true)
                    {
                        DataTypeId = propType.DataTypeId
                    });

                    var content = new DetachedPublishedContent(entity.GetType().Name, publishedContentType, props, null, 1, true);

                    foreach (var property in content.Properties.OfType<DetachedPublishedProperty>())
                    {
                        object source = null;
                        try
                        {
                            var item = rawValue.FirstOrDefault(i => i["alias"].Value<string>().Equals(property.PropertyTypeAlias));
                            
                            source = item?["value"].Value<string>();
                            property.SetValue(source);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(entity.GetType(), $"Error parsing custom {property.PropertyTypeAlias} property with value '{source}'", ex);
                        }
                    }

                    return content;
                }
                catch (Exception ex)
                {
                    LogHelper.Error(entity.GetType(), $"Error parsing custom props for {entity} property", ex);
                }
            }

            return null;
        }

        internal static void Save(this ExtendableEntityBase entity)
        {
            var customProps = entity.CustomProperties?.Properties.OfType<DetachedPublishedProperty>().ToList();

            if (customProps != null && customProps.Any())
            {
                var jProps = new JArray();
                foreach (var prop in customProps)
                {
                    if (prop.Value != null)
                    {
                        jProps.Add(new JObject(new
                        {
                            alias = prop.PropertyTypeAlias,
                            value = prop.Value,
                            id = prop.DataTypeId
                        }));
                    }
                }

                entity.CustomProps = JsonConvert.SerializeObject(jProps);
            }
        }

        public static T CustomPropertiesAs<T>(this ExtendableEntityBase entity) where T : class, IPublishedContent
        {
            var tContent = entity.CustomProperties as T;
            if (tContent != null)
            {
                return tContent;
            }

            if (PublishedContentModelFactoryResolver.Current.HasValue)
            {
                // make a typed model
                return PublishedContentModelFactoryResolver.Current.Factory.CreateModel(entity.CustomProperties) as T;
            }

            return default(T);
        }

        internal static void ParseCustomProps(this IEnumerable<ExtendableEntityBase> entities)
        {
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    entity.ParseCustomProps();
                }
            }
        }
    }
}