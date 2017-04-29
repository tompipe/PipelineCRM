using Newtonsoft.Json.Linq;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.Extensions;
using GrowCreate.PipelineCRM.Services.DataServices;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace GrowCreate.PipelineCRM.Services
{
    public class SegmentService
    {        
        public Segment GetById(int Id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.Id == Id);
            var Segment = SegmentDbService.Instance.Fetch(query).FirstOrDefault();
            return Segment;
        }

        public IEnumerable<Segment> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => !x.Archived);
            var Segments = SegmentDbService.Instance.Fetch(query);
            return Segments;
        }

        public PagedSegments GetPagedSegments(int pageNumber, string sortColumn, string sortOrder, string searchTerm, int typeId)
        {
            int itemsPerPage = PipelineConfig.GetConfig().AppSettings.PageSize;
            var items = new List<Segment>();
            var SegmentType = typeof(Segment);

            var query = new Sql().Select("*").From("pipelineSegment");

            if (typeId == 0)
            {
                query.Append(" where Archived=0 ", typeId);
            }
            else if (typeId == -1)
            {
                query.Append(" where Archived=1 ", typeId);
            }
            else if (typeId == -2)
            {
                query.Append(" where TypeId=0 and Archived=0 ", typeId);
            }
            else 
            {
                query.Append(" where TypeId=@0 and Archived=0 ", typeId);                
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query.Append(" and (Name like @0) ", "%" + searchTerm + "%");                
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
            {
                query.OrderBy(sortColumn + " " + sortOrder);
            }
            else
            {
                query.OrderBy("Name asc");
            }

            var p = SegmentDbService.Instance.Page(pageNumber, itemsPerPage, query);
            return new PagedSegments
            {
                TotalPages = p.TotalPages,
                TotalItems = p.TotalItems,
                ItemsPerPage = p.ItemsPerPage,
                CurrentPage = p.CurrentPage,
                Segments = p.Items.ToList()
            };            
        }  
        
        public IEnumerable<Contact> GetSegmentContacts(Segment segment)
        {
            if (segment.Criteria.ToLower() == "Select contacts")
            {
                return new ContactService().GetByIds(segment.ContactIds);
            }
            if (segment.Criteria.ToLower() == "Select organisations")
            {
                var organisations = new OrganisationService().GetByIds(segment.OrganisationIds);
                return organisations.Select(x => new ContactService().GetByOrganisationId(x.Id)).SelectMany(x => x).Distinct();
            }
            return Enumerable.Empty<Contact>();
        }            

        public Segment Save(Segment Segment)
        {
            return SegmentDbService.Instance.Save(Segment);
        }

        public int Delete(int SegmentId)
        {
            return SegmentDbService.Instance.Delete(SegmentId);
        }
    }
}