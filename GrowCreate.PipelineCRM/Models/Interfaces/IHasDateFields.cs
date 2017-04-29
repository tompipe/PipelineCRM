using System;

namespace GrowCreate.PipelineCRM.Models
{
    internal interface IHasDateFields : IPipelineEntity
    {
        DateTime DateCreated { get; set; }
        DateTime DateUpdated { get; set; }
    }
}