using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RouterSite.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly RouteService _routeService;

        public RoutesController(RouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Route>>> Get()
        {
            return (await _routeService.GetRoutes()).ToArray();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Route route)
        {
            var path = route.Path.StartsWith("/")
                ? route.Path
                : "/" + route.Path;
            
            await _routeService.UpsertRoute(path?.ToLower(), route.Destination?.ToLower());
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            using var reader = new System.IO.StreamReader(Request.Body);
            var path = await reader.ReadToEndAsync();
            await _routeService.DeleteRoute(path);
            return Ok();
        }
    }
}
