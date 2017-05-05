using GrowCreate.PipelineCRM.Extensions;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.SegmentCriteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Resolvers;

namespace GrowCreate.PipelineCRM.Services
{
    public static class SegmentCriteriaService
    {
        public static List<ISegmentCriteria> GetSegmentCriteria()
        {
            return SegmentCriteriaResolver.Current.SegmentCriteria.OrderBy(x => x.Name).ToList();
        }
    }
    public static class SegmentCriteriaExtensions
    {
        public static IEnumerable<dynamic> GetCriteriaProperties(this Segment Segment)
        {
            if (!String.IsNullOrEmpty(Segment.CriteriaProps))
                return Newtonsoft.Json.Linq.JArray.Parse(Segment.CriteriaProps) as IEnumerable<dynamic>;
            else
                return new List<dynamic>();
        }

        public static dynamic GetCriteriaProperty(this Segment Segment, string alias)
        {
            var props = Segment.GetCriteriaProperties();
            return props.Where(x => x.alias.ToString().ToLower() == alias.ToLower()).FirstOrDefault();
        }

        public static dynamic GetCriteriaValue(this Segment Segment, string alias)
        {
            var prop = Segment.GetCriteriaProperty(alias);
            if (prop != null && prop.value != null)
            {             
                return prop.value;
            }
            return null;
        }
    }
}