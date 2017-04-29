using GrowCreate.PipelineCRM.Models;

namespace GrowCreate.PipelineCRM.Services.DataServices
{    
    public class SegmentDbService : DbServiceBase<Segment>
    {
        private SegmentDbService() { }

        public static SegmentDbService Instance { get; } = new SegmentDbService();
    }
}