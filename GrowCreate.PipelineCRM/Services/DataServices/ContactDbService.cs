using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.Services.DataServices
{
    public class ContactDbService : DbServiceBase<Contact>
    {
        private ContactDbService() { }

        public static ContactDbService Instance { get; } = new ContactDbService();
    }
}