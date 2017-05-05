using System;
using System.Collections.Generic;
using GrowCreate.PipelineCRM.SegmentCriteria;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace GrowCreate.PipelineCRM.Resolvers
{
    public class SegmentCriteriaResolver : LazyManyObjectsResolverBase<SegmentCriteriaResolver, ISegmentCriteria>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="criteria"></param>
        /// <remarks>
        /// </remarks>
        public SegmentCriteriaResolver(ILogger logger, Func<IEnumerable<Type>> criteria)
            : base(new SegmentCriteriaServiceProvider(), logger, criteria, ObjectLifetimeScope.HttpRequest)
        {			
        }

        /// <summary>
        /// Gets the migrations
        /// </summary>
        public IEnumerable<ISegmentCriteria> SegmentCriteria
        {
            get { return Values; }
        }


        /// <summary>
        /// This will ctor the IMigration instances
        /// </summary>
        /// <remarks>
        /// This is like a super crappy DI - in v8 we have real DI
        /// </remarks>
        private class SegmentCriteriaServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                var normalArgs = new Type[0];

                var found = serviceType.GetConstructor(normalArgs);
                if (found != null)
                    return found.Invoke(new object[0]);

                //use normal ctor
                return Activator.CreateInstance(serviceType);
            }
        }

        public static void Configure(ILogger logger = null)
        {
            if (logger == null)
            {
                logger = LoggerResolver.Current.Logger;
            }

            var migrationTypes = PluginManager.Current.ResolveTypes<ISegmentCriteria>();

            Current = new SegmentCriteriaResolver(logger, () => migrationTypes);
        }
    }
}