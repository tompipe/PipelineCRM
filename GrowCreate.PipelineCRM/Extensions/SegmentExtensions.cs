using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services.DataServices;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class SegmentExtensions
    {
        public static Segment Save(this Segment segment)
        {
            return SegmentDbService.Instance.Save(segment);
        }

        public static void Delete(this Segment segment)
        {
            SegmentDbService.Instance.Delete(segment.Id);
        }
    }
}