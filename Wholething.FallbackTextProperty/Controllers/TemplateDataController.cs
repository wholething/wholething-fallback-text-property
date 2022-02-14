using System.ComponentModel.DataAnnotations;
using Wholething.FallbackTextProperty.Services;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
#else
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
#endif

namespace Wholething.FallbackTextProperty.Controllers
{
    [PluginController("FallbackText")]
    public class TemplateDataController : UmbracoApiController
    {
        private readonly IFallbackTextService _fallbackTextService;

        public TemplateDataController(IFallbackTextService fallbackTextService)
        {
            _fallbackTextService = fallbackTextService;
        }

#if NET5_0_OR_GREATER
        [HttpGet]
        public IActionResult Get([Required][Range(1, int.MaxValue)] int nodeId, [Required] string propertyAlias, string culture = null)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            return new OkObjectResult(_fallbackTextService.BuildDictionary(nodeId, propertyAlias, culture));
        }
#else
        [HttpGet]
        public IHttpActionResult Get([Required][Range(1, int.MaxValue)] int nodeId, [Required] string propertyAlias, string culture = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(_fallbackTextService.BuildDictionary(nodeId, propertyAlias, culture));
        }
#endif
    }
}
