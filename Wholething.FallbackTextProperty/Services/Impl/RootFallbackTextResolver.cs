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
    public class RootFallbackTextResolver : FallbackTextResolver
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public RootFallbackTextResolver(IFallbackTextLoggerService logger, IPublishedSnapshotAccessor publishedSnapshotAccessor) : base(logger)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
        }

        protected override string FunctionName => "root";
        protected override bool RequireContent => false;

        public override void CheckArguments(string[] args, FallbackTextResolverContext context)
        {
            base.CheckArguments(args, context);

            if (args.Length > 0)
            {
                throw new ArgumentException("Did not expect any arguments");
            }
        }

        protected override IPublishedContent Resolve(string[] args, FallbackTextResolverContext context)
        {
            var snapshot = _publishedSnapshotAccessor.GetPublishedSnapshot();
            return snapshot.Content.GetAtRoot().First();
        }
    }
}
