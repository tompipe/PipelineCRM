﻿using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace GrowCreate.PipelineCRM.Extensions
{
    internal static class PreValueCollectionExtensions
    {
        public static IDictionary<string, string> AsPreValueDictionary(this PreValueCollection preValue)
        {
            return preValue.PreValuesAsDictionary.ToDictionary(x => x.Key, x => x.Value.Value);
        }
    }
}