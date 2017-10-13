using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Models.DetachedPublishedContent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class ExtendableEntityExtensions
    {
        internal static IPublishedContent ParseCustomProps(this ExtendableEntityBase entity)
        {
            if (!string.IsNullOrEmpty(entity?.CustomProps))
            {
                try
                {
                    var rawValue = JsonConvert.DeserializeObject<List<JObject>>(entity.CustomProps);
                    
                    var content = CreateEmptyContent(entity);

                    var detachedPublishedProperties = content?.Properties?.OfType<DetachedPublishedProperty>();
                    if (detachedPublishedProperties != null)
                        foreach (var property in detachedPublishedProperties)
                        {
                            object source = null;
                            try
                            {
                                var item = rawValue?.FirstOrDefault(i =>
                                {
                                    if (i == null) return false;

                                    var value = i["alias"]?.Value<string>();
                                    return value != null && value.Equals(property.PropertyTypeAlias);
                                });

                                source = item?["value"].Value<string>();
                                if (source != null)
                                {
                                    property.SetValue(source);
                                }
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

        private static IPublishedContent CreateEmptyContent<T>(this T entity) where T : ExtendableEntityBase
        {
            var contentType = entity.GetMappedContentType<T>();
            if (contentType == null)
            {
                return null;
            }

            var publishedContentType = PublishedContentType.Get(PublishedItemType.Content, contentType.Alias);
            if (publishedContentType == null)
            {
                return null;
            }

            var props = publishedContentType.PropertyTypes?.Select(propType => propType?.DataTypeId != null
                ? new DetachedPublishedProperty(propType, null, true)
                {
                    DataTypeId = (int) propType?.DataTypeId
                }
                : null);

            var content = new DetachedPublishedContent(entity?.GetType().Name, publishedContentType, props, null, 1, true);

            return content;
        }

        internal static IContentType GetMappedContentType(Type t)
        {
            var mappings = PipelineConfig.GetConfig()?.AppSettings?.ContentTypeMappings;

            var element = mappings?[t];

            return element != null ? ApplicationContext.Current?.Services?.ContentTypeService?.GetContentType(element.Alias) : null;
        }

        internal static IContentType GetMappedContentType<T>(this T entity) where T : ExtendableEntityBase
        {
            return entity != null ? GetMappedContentType(entity.GetType()) : null;
        }

        internal static DocumentTypeDisplay GetScaffold<T>(this T entity) where T : ExtendableEntityBase
        {
            if (entity == null) return null;

            var contentType = GetMappedContentType(entity.GetType());
            return new ContentTypeController(UmbracoContext.Current).GetEmpty(contentType.Id);
        }

        internal static void SaveCustomProperties<T>(this T entity) where T : ExtendableEntityBase
        {
            var contentType = GetMappedContentType(entity?.GetType());
            var jProps = new JArray();

            if (contentType?.PropertyTypes == null) return;

            var update = false;

            foreach (var ctProperty in contentType.PropertyTypes)
            {
                var pubProperty = entity?.CustomProperties?.GetProperty(ctProperty.Alias) as DetachedPublishedProperty;

                if (pubProperty?.HasValue != true) continue;

                jProps.Add(JObject.FromObject(new
                {
                    alias = pubProperty.PropertyTypeAlias,
                    value = pubProperty.DataValue?.ToString(),
                    id = ctProperty.Id
                }));

                update = true;
            }

            if (update)
            {
                entity.CustomProps = JsonConvert.SerializeObject(jProps);
            }
        }

        public static void SetPropertyValue<T>(this T entity, string alias, object value) where T : ExtendableEntityBase
        {
            if (entity?.CustomProperties == null)
            {
                if (entity != null) entity.CustomProperties = CreateEmptyContent(entity);
            }

            (entity?.CustomProperties?.GetProperty(alias) as DetachedPublishedProperty)?.SetValue(value);
        }

        public static T CustomPropertiesAs<T>(this ExtendableEntityBase entity) where T : class, IPublishedContent
        {
            var tContent = entity?.CustomProperties as T;
            if (tContent != null)
            {
                return tContent;
            }

            if (PublishedContentModelFactoryResolver.Current?.HasValue == true)
            {
                // make a typed model
                if (entity?.CustomProperties != null)
                {
                    return PublishedContentModelFactoryResolver.Current?.Factory?.CreateModel(entity.CustomProperties) as T;
                }
            }

            return default(T);
        }

        internal static void ParseCustomProps(this IEnumerable<ExtendableEntityBase> entities)
        {
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (entity != null) entity.CustomProperties = entity.ParseCustomProps();
                }
            }
        }

        public static string GetContentTypeAlias(Type type)
        {
            return GetMappedContentType(type)?.Alias;
        }
    }
}
