using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.Services.DataServices
{    
    public class PipelineDbService : DbServiceBase<Pipeline>
    {
        private PipelineDbService() { }

        public static PipelineDbService Instance { get; } = new PipelineDbService();
    }
}