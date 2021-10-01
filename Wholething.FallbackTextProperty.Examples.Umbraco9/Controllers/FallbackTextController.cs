using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Wholething.FallbackTextProperty.Services;

namespace Wholething.FallbackTextProperty.Examples.Umbraco9.Controllers
{
    [PluginController("FallbackText")]
    public class TemplateDataController : UmbracoApiController
    {
        private readonly IFallbackTextService _fallbackTextService;

        public TemplateDataController(IFallbackTextService fallbackTextService)
        {
            _fallbackTextService = fallbackTextService;
        }

        [HttpGet]
        public Dictionary<string, object> Get(int nodeId, string propertyAlias)
        {
            return _fallbackTextService.BuildDictionary(nodeId, propertyAlias);
        }
    }
}
