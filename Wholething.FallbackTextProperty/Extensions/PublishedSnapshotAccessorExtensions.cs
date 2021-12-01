#if NET5_0_OR_GREATER
using Umbraco.Cms.Core.PublishedCache;
#else
using Umbraco.Web.PublishedCache;
#endif

namespace Wholething.FallbackTextProperty.Extensions
{
    public static class PublishedSnapshotAccessorExtensions
    {
        public static IPublishedSnapshot GetPublishedSnapshot(this IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
#if NET5_0_OR_GREATER
            publishedSnapshotAccessor.TryGetPublishedSnapshot(out var publishedSnapshot);
            return publishedSnapshot;
#else
            return publishedSnapshotAccessor.PublishedSnapshot;
#endif
        }
    }
}
