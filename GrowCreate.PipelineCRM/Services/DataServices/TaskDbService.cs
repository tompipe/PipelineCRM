using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.Services.DataServices
{
    public class TaskDbService : DbServiceBase<Task>
    {
        private TaskDbService() { }

        public static TaskDbService Instance { get; } = new TaskDbService();
    }
}