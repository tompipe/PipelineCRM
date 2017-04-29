using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class OrganisationExtensions
    {
        public static Organisation Save(this Organisation Organisation)
        {
            return new OrganisationService().Save(Organisation);
        }

        public static void Delete(this Organisation Organisation)
        {
            new OrganisationService().Delete(Organisation.Id);
        }

    }
}