using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class ContactExtensions
    {
        public static Contact Save(this Contact contact)
        {
            return new ContactService().Save(contact);
        }

        public static void Delete(this Contact contact)
        {
            new ContactService().Delete(contact.Id);
        }
    }
}