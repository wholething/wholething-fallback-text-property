using System;
using System.Linq;
using Wholething.FallbackTextProperty.Extensions;
using Wholething.FallbackTextProperty.Services.Models;
#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Models.PublishedContent;
#else
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Models.PublishedContent;
#endif

namespace Wholething.FallbackTextProperty.Services.Impl
{
    public class UrlFallbackTextResolver : FallbackTextResolver
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public UrlFallbackTextResolver(IFallbackTextLoggerService logger, IPublishedSnapshotAccessor publishedSnapshotAccessor) : base(logger)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
        }

        protected override string FunctionName => "url";
        protected override bool RequireContent => false;

        public override void CheckArguments(string[] args, FallbackTextResolverContext context)
        {
            base.CheckArguments(args, context);

            if (args.Length != 1)
            {
                throw new ArgumentException("Expected exactly 1 argument");
            }
        }

        protected override IPublishedContent Resolve(string[] args, FallbackTextResolverContext context)
        {
            var snapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            return snapshot.Content.GetByRoute(args[0]);
        }
    }
}
