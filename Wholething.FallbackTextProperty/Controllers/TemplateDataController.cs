using System;
using System.ComponentModel.DataAnnotations;
using Wholething.FallbackTextProperty.Services;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
#else
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
#endif

namespace Wholething.FallbackTextProperty.Controllers
{
    [PluginController("FallbackText")]
    public class TemplateDataController : UmbracoAuthorizedApiController
    {
        private readonly IFallbackTextService _fallbackTextService;

        public TemplateDataController(IFallbackTextService fallbackTextService)
        {
            _fallbackTextService = fallbackTextService;
        }

#if NET5_0_OR_GREATER
        [HttpGet]
        public IActionResult Get([Required] Guid nodeId, [Required] Guid dataTypeKey, string culture = null, Guid? blockId = null)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            return new OkObjectResult(_fallbackTextService.BuildDictionary(nodeId, blockId, dataTypeKey, culture));
        }
#else
        [HttpGet]
        public IHttpActionResult Get([Required] Guid nodeId, [Required] Guid dataTypeKey, string culture = null, Guid? blockId = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(_fallbackTextService.BuildDictionary(nodeId, blockId, dataTypeKey, culture));
        }
#endif
    }
}
