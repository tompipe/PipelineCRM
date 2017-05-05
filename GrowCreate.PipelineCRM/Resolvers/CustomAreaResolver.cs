using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GrowCreate.PipelineCRM.CustomAreas;
using GrowCreate.PipelineCRM.SegmentCriteria;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace GrowCreate.PipelineCRM.Resolvers
{
    public class CustomAreaResolver : LazyManyObjectsResolverBase<CustomAreaResolver, ICustomArea>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="criteria"></param>
        /// <remarks>
        /// </remarks>
        public CustomAreaResolver(ILogger logger, Func<IEnumerable<Type>> criteria)
            : base(new CustomAreaServiceProvider(), logger, criteria)
        {			
        }

        /// <summary>
        /// Gets the migrations
        /// </summary>
        public IEnumerable<ICustomArea> CustomAreas
        {
            get { return Values; }
        }


        /// <summary>
        /// This will ctor the IMigration instances
        /// </summary>
        /// <remarks>
        /// This is like a super crappy DI - in v8 we have real DI
        /// </remarks>
        private class CustomAreaServiceProvider : IServiceProvider
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

            var customAreaTypes = PluginManager.Current.ResolveTypes<ICustomArea>();

            Current = new CustomAreaResolver(logger, () => customAreaTypes);
        }
    }
}