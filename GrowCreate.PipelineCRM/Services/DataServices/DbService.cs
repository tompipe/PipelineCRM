using System;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace GrowCreate.PipelineCRM.Services.DataServices
{
    public class DbService
    {
        [ThreadStatic]
        private static volatile Database _db;

        internal static Database db()
        {
            return _db ?? (_db = ApplicationContext.Current.DatabaseContext.Database);
        }
    }
}