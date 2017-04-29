using System;
using System.Collections.Generic;
using System.Linq;
using GrowCreate.PipelineCRM.Extensions;
using GrowCreate.PipelineCRM.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace GrowCreate.PipelineCRM.Services.DataServices
{
    public abstract class DbServiceBase<T> where T : IPipelineEntity
    {
        public event TypedEventHandler<DbServiceBase<T>, SaveEventArgs<T>> Saving;
        public event TypedEventHandler<DbServiceBase<T>, SaveEventArgs<T>> Saved;
        public event TypedEventHandler<DbServiceBase<T>, DeleteEventArgs<T>> Deleting;
        public event TypedEventHandler<DbServiceBase<T>, DeleteEventArgs<T>> Deleted;

        [Obsolete]
        public event EventHandler OnBeforeSave;

        [Obsolete]
        public event EventHandler OnAfterSave;

        protected void OnSaving(T entity)
        {
            UpdateDates(entity as IHasDateFields);
            UpdateProperties(entity as ExtendableEntityBase);

            OnBeforeSave?.Invoke(entity, null);
            Saving?.Invoke(this, new SaveEventArgs<T>(entity));
        }

        private void UpdateProperties(ExtendableEntityBase hasCustomProperties)
        {
            hasCustomProperties?.Save();
        }

        private void UpdateDates(IHasDateFields entity)
        {
            if (entity != null)
            {
                entity.DateUpdated = DateTime.Now;

                if (entity.Id == 0)
                {
                    entity.DateCreated = entity.DateUpdated;
                }
            }
        }

        protected void OnSaved(T entity)
        {
            OnAfterSave?.Invoke(entity, null);
            Saved?.Invoke(this, new SaveEventArgs<T>(entity));
        } 

        protected void OnDeleting(T entity)
        {
            Deleting?.Invoke(this, new DeleteEventArgs<T>(entity));
        } 

        protected void OnDeleted(T entity)
        {
            Deleted?.Invoke(this, new DeleteEventArgs<T>(entity));
        } 

        public int Delete(int contactId)
        {
            return DbService.db().Delete<T>(contactId);
        }

        public T Save(T entity)
        {
            OnSaving(entity);

            if (entity.Id > 0)
            {
                Db().Update(entity);
            }
            else
            {
                Db().Save(entity);
            }

            OnSaved(entity);

            return entity;
        }

        protected Database Db()
        {
            return DbService.db();
        }

        public List<T> Fetch(Sql query)
        {
            var list = Db().Fetch<T>(query);
            (list as IEnumerable<ExtendableEntityBase>).ParseCustomProps();

            return list;
        }
        
        public IEnumerable<T> Query(string sql, params object[] args)
        {
            var list = Db().Query<T>(sql, args);
            (list as IEnumerable<ExtendableEntityBase>).ParseCustomProps();

            return list;
        }

        public Page<T> Page(int pageNumber, int itemsPerPage, Sql query)
        {
            var list = Db().Page<T>(pageNumber, itemsPerPage, query);
            (list.Items as IEnumerable<ExtendableEntityBase>).ParseCustomProps();

            return list;
        }
    }
}