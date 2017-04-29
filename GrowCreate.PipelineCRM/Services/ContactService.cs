using Newtonsoft.Json.Linq;
using GrowCreate.PipelineCRM.Config;
﻿using GrowCreate.PipelineCRM.Config;
using GrowCreate.PipelineCRM.Controllers;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;
using GrowCreate.PipelineCRM.Services.DataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Core;

namespace GrowCreate.PipelineCRM.Services
{
    public class ContactService
    {        
        public Contact GetById(int Id, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Id == Id);
            var contact = ContactDbService.Instance.Fetch(query).FirstOrDefault();            

            if (getLinks && contact != null && !contact.Obscured)
            {
                return GetLinks(contact);
            }
            return contact;
        }

        public IEnumerable<Contact> GetByIds(string Ids = "")
        {
            if (!String.IsNullOrEmpty(Ids) && Ids.Split(',').Count() > 0)
            {
                int[] idList = Ids.Split(',').Select(int.Parse).ToArray();
                var query = new Sql("select * from pipelineContact where Id in (@idList)", new { idList });
                return ContactDbService.Instance.Fetch(query);
            }
            return new List<Contact>();
        }

        public Contact GetByEmail(string Email, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.Email == Email);
            var contact = ContactDbService.Instance.Fetch(query).FirstOrDefault();            

            if (getLinks && contact != null && !contact.Obscured)
            {
                return GetLinks(contact);
            }
            return contact;
        }

        public IEnumerable<Contact> GetAll(bool getLinks = false)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => !x.Archived);
            var contacts = ContactDbService.Instance.Fetch(query);
            if (getLinks)
            {
                for (int i = 0; i < contacts.Count(); i++)
                {
                    contacts[i] = GetLinks(contacts[i]);
                }
            }

            return contacts;
        }

        public PagedContacts GetPagedContacts(int pageNumber, string sortColumn, string sortOrder, string searchTerm, int typeId)
        {
            int itemsPerPage = PipelineConfig.GetConfig().AppSettings.PageSize;
            var items = new List<Contact>();
            var contactType = typeof(Contact);

            var query = new Sql().Select("*").From("pipelineContact");

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
                query.Append(" and (Name like @0 or Email like @0) ", "%" + searchTerm + "%");                
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortOrder) && sortColumn != "OrganisationNames")
            {
                query.OrderBy(sortColumn + " " + sortOrder);
            }
            else
            {
                query.OrderBy("Name asc");
            }

            var p = ContactDbService.Instance.Page(pageNumber, itemsPerPage, query);

            for (int i = 0; i < p.Items.ToList().Count(); i++)
            {
                p.Items[i].Organisations = new OrganisationApiController().GetByIds(p.Items[i].OrganisationIds);
                p.Items[i].OrganisationNames = p.Items[i].Organisations.Select(x => x.Name).OrderBy(x => x);
            }

            // special sorting for organisations
            if (sortColumn == "OrganisationNames")
            {
                p.Items = sortOrder.ToLower() != "desc" ?
                    p.Items.OrderBy(x => x.OrganisationNames.FirstOrDefault()).ToList() : p.Items.OrderByDescending(x => x.OrganisationNames.FirstOrDefault()).ToList();
            }

            return new PagedContacts
            {
                TotalPages = p.TotalPages,
                TotalItems = p.TotalItems,
                ItemsPerPage = p.ItemsPerPage,
                CurrentPage = p.CurrentPage,
                Contacts = p.Items.ToList()
            };            
        }

        public IEnumerable<Contact> GetByOrganisationId(int OrganisationId, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.OrganisationIds != "" && x.OrganisationIds.Contains(OrganisationId.ToString()));
            var contacts = ContactDbService.Instance.Fetch(query);
            if (getLinks)
            {
                for (int i = 0; i < contacts.Count(); i++)
                {
                    contacts[i] = GetLinks(contacts[i]);
                }
            }
            return contacts;
        }

        public IEnumerable<Contact> GetByTypeId(int TypeId, bool getLinks = true)
        {
            var query = new Sql().Select("*").From("pipelineContact").Where<Contact>(x => x.TypeId == TypeId);
            var contacts = ContactDbService.Instance.Fetch(query);
            if (getLinks)
            {
                for (int i = 0; i < contacts.Count(); i++)
                {
                    contacts[i] = GetLinks(contacts[i]);
                }
            }
            return contacts;
        }

        public Contact Save(Contact contact)
        {
            return ContactDbService.Instance.Save(contact);
        }

        public int Delete(int ContactId)
        {
            return ContactDbService.Instance.Delete(ContactId);
        }

        public Contact GetLinks(Contact contact)
        {
            if (!String.IsNullOrEmpty(contact.OrganisationIds))
            {
                contact.Organisations = new OrganisationApiController().GetByIds(contact.OrganisationIds);
            }
            contact.Tasks = new TaskApiController().GetByContactId(contact.Id);
            contact.Type = new ContactTypeApiController().GetById(contact.TypeId);

            if (!String.IsNullOrEmpty(contact.Files) && contact.Files.Split(',').Count() > 0)
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var media = umbracoHelper.TypedMedia(contact.Files.Split(','));
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

                contact.Media = mediaList;
            }

            return contact;
        }
    }
}