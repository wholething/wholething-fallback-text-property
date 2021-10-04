using System.ComponentModel.DataAnnotations;
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
        public IActionResult Get([Required][Range(1, int.MaxValue)] int nodeId, [Required] string propertyAlias)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            return new OkObjectResult(_fallbackTextService.BuildDictionary(nodeId, propertyAlias));
        }
    }
}
