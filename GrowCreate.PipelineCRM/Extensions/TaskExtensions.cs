using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services.DataServices;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class TaskExtensions
    {
        public static Task Save(this Task t)
        {
            return TaskDbService.Instance.Save(t); 
        }

        public static int Delete(this Task t)
        {
            return TaskDbService.Instance.Delete(t.Id);
        }
    }
}