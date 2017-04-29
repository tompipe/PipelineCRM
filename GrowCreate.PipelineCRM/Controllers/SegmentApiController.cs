﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Extensions;
using GrowCreate.PipelineCRM.Services.DataServices;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class SegmentApiController : UmbracoAuthorizedApiController
    {
        public Segment GetLinks(Segment segment)
        {
            if (!String.IsNullOrEmpty(segment.OrganisationIds))
            {
                segment.Organisations = new OrganisationApiController().GetByIds(segment.OrganisationIds);
            }
            if (!String.IsNullOrEmpty(segment.ContactIds))
            {
                segment.Contacts = new ContactApiController().GetByIds(segment.ContactIds);
            }
            segment.Tasks = new TaskApiController().GetBySegmentId(segment.Id);                        
            return segment;
        }

        public IEnumerable<Segment> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => !x.Archived);
            return SegmentDbService.Instance.Fetch(query).OrderBy(x => x.Name);
        }

        public PagedSegments GetPaged(int pageNumber = 0, string sortColumn = "", string sortOrder = "", string searchTerm = "", int typeId = 0)
        {
            return new SegmentService().GetPagedSegments(pageNumber, sortColumn, sortOrder, searchTerm, typeId);
        }

        public IEnumerable<Segment> GetUnassigned(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => !x.Archived && x.TypeId == 0);
            var segments = SegmentDbService.Instance.Fetch(query);
            return segments.OrderBy(x => x.Name);
        }               

        public Segment GetById(int id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.Id == id);
            var segment = SegmentDbService.Instance.Fetch(query).FirstOrDefault();
            if (getLinks && segment != null)
            {
                segment = GetLinks(segment);
            }
            return segment;
        }

        public IEnumerable<Segment> GetByIds(string Ids = "")
        {
            if (!String.IsNullOrEmpty(Ids) && Ids.Split(',').Count() > 0)
            {
                int[] idList = Ids.Split(',').Select(int.Parse).ToArray();
                var query = new Sql("select * from pipelineSegment where Id in (@idList)", new { idList }); //.Select("*").From("pipelineSegment").Where<Segment>(x => Ids.Split(',').Contains(x.Id.ToString()));
                return SegmentDbService.Instance.Fetch(query);
            }
            return new List<Segment>();
        }

        public IEnumerable<Segment> GetByContactId(int id)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.ContactIds.Contains(id.ToString()));
            return SegmentDbService.Instance.Fetch(query);
        }

        public IEnumerable<Segment> GetByTypeId(int id, bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.TypeId == id).Where<Segment>(x => !x.Archived);
            return SegmentDbService.Instance.Fetch(query);
        }

        public IEnumerable<string> GetCriteria()
        {
            return SegmentCriteriaService.GetSegmentCriteria().Select(x => x.Name).ToList();
        }

        public IEnumerable<Contact> GetContacts(int id, string criteria = "")
        {
            var segment = GetById(id);
            return GetContacts(segment, criteria);
        }

        public IEnumerable<Contact> GetContacts(Segment segment, string criteria = "")
        {
            criteria = !string.IsNullOrEmpty(criteria) ? criteria : segment.Criteria;
            var _criteria = SegmentCriteriaService.GetSegmentCriteria().SingleOrDefault(x => x.Name == criteria);
            return _criteria.GetContacts(segment);
        }

        public Segment GetByName(string name)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.Name.ToLower() == name.ToLower());
            var org = SegmentDbService.Instance.Fetch(query).FirstOrDefault();
            return org;
        }

        public IEnumerable<Segment> GetSegmentsByName(string name)
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.Name.ToLower().Contains(name.ToLower()));
            return SegmentDbService.Instance.Fetch(query);
        }

        public Segment PostSave(Segment seg)
        {
            seg.Save();
            return seg;
        }

        public IEnumerable<Segment> PostSaveSegments(IEnumerable<Segment> segments)
        {
            foreach (var org in segments)
            {
                PostSave(org);
            }
            return segments;
        }

        public int DeleteById(int id, bool deleteLinks = false)
        {
            var org = GetById(id);
            org?.Delete();

            return 0;
        }

        public void DeleteSegments(IEnumerable<Segment> segments, bool deleteLinks = false)
        {
            foreach (var org in segments)
            {
                DeleteById(org.Id);
            }
        }

        public void DeleteSegmentsById(string Ids, bool deleteLinks = false)
        {
            var idList = Ids.Split(',').Select(s => int.Parse(s));
            foreach (var id in idList)
            {
                DeleteById(id, deleteLinks);
            }
        }

        private IEnumerable<IMember> GetMembers(string MemberIds = "")
        {
            if (!String.IsNullOrEmpty(MemberIds))
            {
                int[] memberIds = Array.ConvertAll(MemberIds.Split(','), int.Parse);
                return ApplicationContext.Current.Services.MemberService.GetAllMembers(memberIds);
            }
            else 
            {
                return new List<IMember>();
            }   
        }

        public IEnumerable<Segment> GetArchived()
        {
            var query = new Sql().Select("*").From("pipelineSegment").Where<Segment>(x => x.Archived);
            var segments = SegmentDbService.Instance.Fetch(query);
            return segments;
        }

        public void Archive(Segment org)
        {
            org.Archived = true;
            SegmentDbService.Instance.Save(org);
        }

        public void Restore(Segment org)
        {
            org.Archived = false;
            SegmentDbService.Instance.Save(org);
        }

    }
}