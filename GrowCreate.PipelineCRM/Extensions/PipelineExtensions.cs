using GrowCreate.PipelineCRM.Controllers;
using GrowCreate.PipelineCRM.Models;
using GrowCreate.PipelineCRM.Services.DataServices;

namespace GrowCreate.PipelineCRM.Extensions
{
    public static class PipelineExtensions
    {
        public static Pipeline Save(this Pipeline pipeline)
        {
            return PipelineDbService.Instance.Save(pipeline);
        }
    }
}