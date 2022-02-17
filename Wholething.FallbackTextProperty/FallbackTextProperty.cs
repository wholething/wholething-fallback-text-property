#if NET5_0_OR_GREATER
using System;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;
#else
using Umbraco.Core.Migrations;
#endif

namespace Wholething.FallbackTextProperty
{
    /// <summary>
    ///  we only have this class, so there is a dll in the root
    ///  FallbackTextProperty package.
    ///  
    ///  With a root dll, the package can be stopped from installing
    ///  on .netframework sites.
    /// </summary>
    public static class FallbackTextProperty
    {
        public static string PackageName = "Fallback Text Property";
    }

    /// <summary>
    ///  A package migration plan, allows us to put FallbackTextProperty in the list 
    ///  of installed packages. we don't actually need a migration 
    ///  for FallbackTextProperty (doesn't add anything to the db). but by doing 
    ///  this people can see that it is installed. 
    /// </summary>
#if NET5_0_OR_GREATER
    public class FallbackTextPropertyMigrationPlan : PackageMigrationPlan
    {
        public FallbackTextPropertyMigrationPlan() :
            base(FallbackTextProperty.PackageName)
        { }

        protected override void DefinePlan()
        {
            To<SetupFallbackTextProperty>(new Guid("0f55468d-d1bb-4ae5-ad4b-900807634894"));
        }
    }

    public class SetupFallbackTextProperty : PackageMigrationBase
    {
        public SetupFallbackTextProperty(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context)
            : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context)
        {
        }

        protected override void Migrate()
        {
            // we don't actually need to do anything, but this means we end up
            // on the list of installed packages. 
        }
    }
#else
    public class FallbackTextPropertyMigrationPlan : MigrationPlan
    {
        public FallbackTextPropertyMigrationPlan() :
            base(FallbackTextProperty.PackageName)
        {
            To<SetupFallbackTextProperty>("0f55468d-d1bb-4ae5-ad4b-900807634894");
        }

        
    }

    public class SetupFallbackTextProperty : MigrationBase
    {
        public SetupFallbackTextProperty(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            // we don't actually need to do anything, but this means we end up
            // on the list of installed packages. 
        }
    }
#endif
}
