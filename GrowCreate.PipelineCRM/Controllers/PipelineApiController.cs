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
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core;
using GrowCreate.PipelineCRM.Services;
using Newtonsoft.Json.Linq;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Extensions;
using GrowCreate.PipelineCRM.Services.DataServices;

namespace GrowCreate.PipelineCRM.Controllers
{
    [PluginController("PipelineCRM")]
    public class PipelineApiController : UmbracoAuthorizedApiController
    {
        public Pipeline GetLinks(Pipeline pipeline)
        {
            pipeline.Status = new StatusApiController().GetById(pipeline.StatusId);            
            pipeline.Organisation = new OrganisationApiController().GetById(pipeline.OrganisationId, false);
            pipeline.Tasks = new TaskApiController().GetByPipeline(pipeline.Id);
            pipeline.Contact = new ContactApiController().GetById(pipeline.ContactId, false);

            if (pipeline.UserId >= 0)
            {
                var userService = ApplicationContext.Current.Services.UserService;
                var user = userService.GetByProviderKey(pipeline.UserId);
                if (user != null)
                {
                    pipeline.UserName = user.Name;
                    pipeline.UserAvatar = umbraco.library.md5(user.Email);
                }                
            }

            if (!String.IsNullOrEmpty(pipeline.LabelIds))
                pipeline.Labels = new LabelApiController().GetById(pipeline.LabelIds);

            return pipeline;
        }

        public IEnumerable<Pipeline> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => !x.Archived);
            var pipelines = PipelineDbService.Instance.Fetch(query).ToList();

            if (getLinks)
            {
                for (int i = 0; i < pipelines.Count(); i++)
                {
                    pipelines[i] = GetLinks(pipelines[i]);
                }
            }
            return pipelines;
        }

        public PagedPipelines GetPaged(int pageNumber = 0, string sortColumn = "DateComplete", string sortOrder = "desc", string searchTerm = "", int statusId = 0, int contactId = 0, int organisationId = 0)
        {
            return new PipelineService().GetPagedPipelines(pageNumber, sortColumn, sortOrder, searchTerm, statusId, contactId, organisationId);
        }

        public IEnumerable<IUser> GetUsers()
        {
            int allUsers;
            var userService = ApplicationContext.Current.Services.UserService;
            return userService.GetAll(0, 1000, out allUsers).Where(x => x.AllowedSections.Contains("pipelineCrm"));
        }

        public IEnumerable<Pipeline> GetByStatusId(int id, bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.StatusId == id).Where<Pipeline>(x => !x.Archived);
            var pipelines = PipelineDbService.Instance.Fetch(query).ToList();

            if (getLinks)
            {
                for (int i = 0; i < pipelines.Count(); i++)
                {
                    pipelines[i] = GetLinks(pipelines[i]);
                }
            }
            return pipelines;
        }

        public IEnumerable<Pipeline> GetByOrganisationId(int id, bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.OrganisationId == id).Where<Pipeline>(x => !x.Archived);
            var pipelines = PipelineDbService.Instance.Fetch(query).ToList();

            if (getLinks)
            {
                for (int i = 0; i < pipelines.Count(); i++)
                {
                    pipelines[i] = GetLinks(pipelines[i]);
                }
            }
            return pipelines;
        }

        public IEnumerable<Pipeline> GetByUserEmail(string email)
        {
            var query = new Sql("select p.* from pipelinePipeline p, pipelineContact c where p.ContactId = c.Id and c.Email = @0 and (p.Archived is null or p.Archived = 0) and p.ShareWithContact = 1", email);
            var pipelines = PipelineDbService.Instance.Fetch(query).ToList();

            for (int i = 0; i < pipelines.Count(); i++)
            {
                pipelines[i] = GetLinks(pipelines[i]);
            }
            return pipelines;
        }

        public IEnumerable<Pipeline> GetByContactId(int id)
        {
            var query = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.ContactId == id).Where<Pipeline>(x => !x.Archived);
            var pipelines = PipelineDbService.Instance.Fetch(query).ToList();

            for (int i = 0; i < pipelines.Count(); i++)
            {
                pipelines[i] = GetLinks(pipelines[i]);
            }
            return pipelines;
        }

        public IEnumerable<Pipeline> GetCurrent()
        {
            List<int> openStatuses = new StatusApiController().GetOpen().Select(x => x.Id).ToList();
            return GetAll().Where<Pipeline>(x => openStatuses.Contains(x.StatusId));
        }

        public List<double> GetCurrentValue()
        {
            var value = new List<double>();
            var open = GetCurrent();
            value.Add(open.Sum(x => x.Value));
            value.Add(open.Sum(x => x.Value * (x.Probability / 100)));
            return value;
        }

        public IEnumerable<Pipeline> GetLatest(int Months = 6, int StatusId = 0)
        {
            var latest = GetAll().Where<Pipeline>(x => (DateTime.Now - x.DateCreated).Days < Months * 30);
            if (StatusId > 0)
            {
                latest = latest.Where(x => x.StatusId == StatusId);
            }
            return latest;
        }

        public Pipeline GetById(int id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.Id == id);
            var pipeline = PipelineDbService.Instance.Fetch(query).FirstOrDefault();            

            if (pipeline != null && getLinks && !pipeline.Obscured)
            {
                pipeline = GetLinks(pipeline);
            }

            return pipeline;
        }

        public Pipeline QuickCreate(QuickCreate data)
        {
            var newPipeline = new PipelineService().CreateNew(data.ContactName, data.OrganisationName, data.ContactEmail, "", data.Name, data.Message, 0, data.Value, data.Probability, data.StatusId);
            return GetById(newPipeline.Id);
        }

        public Pipeline Duplicate(Pipeline source)
        {
            var query = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.Id == source.Id);
            var pipeline = PipelineDbService.Instance.Fetch(query).FirstOrDefault();
            
            pipeline.Id = 0;
            pipeline.Name = pipeline.Name + " (copy)";
            
            return PostSave(pipeline);
        }

        public Pipeline PostSave(Pipeline pipeline)
        {
            return pipeline.Save();
        }

        public int PostSavePipelines(IEnumerable<Pipeline> pipelines)
        {
            foreach (var pipeline in pipelines)
            {
                PostSave(pipeline);
            }
            return pipelines.Count();
        }
        
        public int DeleteById(int id, bool deleteLinks = false)
        {
            var pipeline = GetById(id);
            if (pipeline != null)
            {
                if (deleteLinks)
                {
                    if (pipeline.ContactId > 0)
                    {
                        new ContactApiController().DeleteById(pipeline.ContactId);
                    }                    
                }

                new TaskApiController().DeleteTasks(pipeline.Tasks);
                return PipelineDbService.Instance.Delete(id);
            }
            return 0;
        }

        public void DeletePipelines(IEnumerable<Pipeline> pipelines, bool deleteLinks = false)
        {
            foreach (var pipeline in pipelines)
            {
                DeleteById(pipeline.Id);
            }
        }

        public void DeletePipelinesById(string Ids, bool deleteLinks = false)
        {
            var idList = Ids.Split(',').Select(s => int.Parse(s));
            foreach (var id in idList)
            {
                DeleteById(id, deleteLinks);
            }
        }

        public IEnumerable<Pipeline> GetArchived()
        {
            var query = new Sql().Select("*").From("pipelinePipeline").Where<Pipeline>(x => x.Archived);
            var pipelines = PipelineDbService.Instance.Fetch(query);
            return pipelines;
        }

        public void Archive(Pipeline pipeline)
        {
            pipeline.Archived = true;
            pipeline.Save();
        }

        public void Restore(Pipeline pipeline)
        {
            pipeline.Archived = false;
            pipeline.Save();
        }

    }

    // pipeline extensions
}