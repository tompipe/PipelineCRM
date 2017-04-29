using Newtonsoft.Json.Linq;
using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Controllers;
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
    public class OrganisationService
    {        
        public Organisation GetById(int Id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.Id == Id);
            var Organisation = OrganisationDbService.Instance.Fetch(query).FirstOrDefault();            

            if (getLinks && Organisation != null && !Organisation.Obscured)
            {
                return GetLinks(Organisation);
            }
            return Organisation;
        }

        public IEnumerable<Organisation> GetByIds(string Ids = "")
        {
            if (!String.IsNullOrEmpty(Ids) && Ids.Split(',').Count() > 0)
            {
                int[] idList = Ids.Split(',').Select(int.Parse).ToArray();
                var query = new Sql("select * from pipelineOrganisation where Id in (@idList)", new { idList }); //.Select("*").From("pipelineOrganisation").Where<Organisation>(x => Ids.Split(',').Contains(x.Id.ToString()));
                return OrganisationDbService.Instance.Fetch(query);
            }
            return new List<Organisation>();
        }

        public IEnumerable<Organisation> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => !x.Archived);
            var Organisations = OrganisationDbService.Instance.Fetch(query);
            if (getLinks)
            {
                for (int i = 0; i < Organisations.Count(); i++)
                {
                    Organisations[i] = GetLinks(Organisations[i]);
                }
            }

            return Organisations;
        }

        public PagedOrganisations GetPagedOrganisations(int pageNumber, string sortColumn, string sortOrder, string searchTerm, int typeId)
        {
            int itemsPerPage = PipelineConfig.GetConfig().AppSettings.PageSize;
            var items = new List<Organisation>();
            var OrganisationType = typeof(Organisation);

            var query = new Sql().Select("*").From("pipelineOrganisation");

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
                query.Append(" and (Name like @0 or Address like @0 or Website like @0 or Email like @0) ", "%" + searchTerm + "%");                
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder))
            {
                query.OrderBy(sortColumn + " " + sortOrder);
            }
            else
            {
                query.OrderBy("Name asc");
            }

            var p = OrganisationDbService.Instance.Page(pageNumber, itemsPerPage, query);
            return new PagedOrganisations
            {
                TotalPages = p.TotalPages,
                TotalItems = p.TotalItems,
                ItemsPerPage = p.ItemsPerPage,
                CurrentPage = p.CurrentPage,
                Organisations = p.Items.ToList()
            };            
        }  
              
        public IEnumerable<Organisation> GetByTypeId(int TypeId, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineOrganisation").Where<Organisation>(x => x.TypeId == TypeId);
            var Organisations = OrganisationDbService.Instance.Fetch(query);
            if (getLinks)
            {
                for (int i = 0; i < Organisations.Count(); i++)
                {
                    Organisations[i] = GetLinks(Organisations[i]);
                }
            }
            return Organisations;
        }

        public Organisation Save(Organisation Organisation)
        {
            return OrganisationDbService.Instance.Save(Organisation);
        }

        public int Delete(int OrganisationId)
        {
            return OrganisationDbService.Instance.Delete(OrganisationId);
        }

        public Organisation GetLinks(Organisation org)
        {
            org.Tasks = new TaskApiController().GetByOrganisationId(org.Id).OrderBy(x => x.Overdue).OrderBy(x => x.DateDue);
            org.Pipelines = new PipelineApiController().GetByOrganisationId(org.Id);
            org.OrgType = new OrgTypeApiController().GetById(org.TypeId);

            if (!String.IsNullOrEmpty(org.Files) && org.Files.Split(',').Count() > 0)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var media = umbracoHelper.TypedMedia(org.Files.Split(','));
                var mediaList = new List<MediaFile>();

                foreach (var file in media)
                {
                    var mediaFile = new MediaFile()
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Url = file.Url
                    };
                    mediaList.Add(mediaFile);
                }

                org.Media = mediaList;
            }
            return org;
        }
    }
}