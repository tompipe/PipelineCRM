using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core.Persistence;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using GrowCreate.PipelineCRM.Models;
using Umbraco.Web.WebApi;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Newtonsoft.Json;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Config;
using System.Dynamic;
using GrowCreate.PipelineCRM.Extensions;
using Newtonsoft.Json.Linq;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class CustomPropertyApiController : UmbracoAuthorizedApiController
    {
        public IEnumerable<CustomPropertyTab> GetCriteriaProps(string criteriaName)
        {
            var criteria = SegmentCriteriaService.GetSegmentCriteria().SingleOrDefault(x => x.Name == criteriaName);
            if (criteria != null)
            {
                return GetCustomProps(docTypeAlias: criteria.ConfigDocType);
            }
            return null;
        }

        public IEnumerable<CustomPropertyTab> GetCustomProps(string type = "", string docTypeAlias = "")
        {
            var outProps = new List<CustomPropertyTab>();

            // we need either param
            if (string.IsNullOrEmpty(docTypeAlias) && string.IsNullOrEmpty(type))
                return outProps;

            IContentType contentType;
            switch (type)
            {
                case "contact":
                    contentType = ExtendableEntityExtensions.GetMappedContentType(typeof(Contact));
                    break;
                case "organisation":
                    contentType = ExtendableEntityExtensions.GetMappedContentType(typeof(Organisation));
                    break;
                case "segment":
                    contentType = ExtendableEntityExtensions.GetMappedContentType(typeof(Segment));
                    break;
                default:
                    contentType = ExtendableEntityExtensions.GetMappedContentType(typeof(Pipeline));
                    break;
            }

            if (contentType == null)
                return outProps;
            
            // TODO: change front end to use the native umbraco DocumentTypeDisplay and then theres no need for CustomPropertyTab / CustomProperty
            var scaffold = new ContentTypeController(UmbracoContext).GetEmpty(contentType.Id);
            var tabs = scaffold.Groups;

            // construct shadow doc type definition

            foreach (var tab in tabs)
            {
                var tabProps = new CustomPropertyTab()
                {
                    name = tab.Name.Contains('.') ? tab.Name.Split('.')[1] : tab.Name,
                    items = new List<CustomProperty>()
                };

                foreach (var prop in tab.Properties)
                {
                    var newProp = new CustomProperty()
                    {
                        id = prop.Id,
                        alias = prop.Alias,
                        label = prop.Label,
                        description = prop.Description,
                        view = prop.View,
                        config = prop.Config,
                        editor = prop.Editor
                    };
                    tabProps.items.Add(newProp);
                }

                outProps.Add(tabProps);               
            }

            return outProps;
        }
    }
}