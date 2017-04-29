using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.Services.DataServices
{    
    public class OrganisationDbService : DbServiceBase<Organisation>
    {
        private OrganisationDbService() { }

        public static OrganisationDbService Instance { get; } = new OrganisationDbService();
    }
}