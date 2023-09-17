using System.Net.Mime;
using AssetTrackingService.Abstract;
using AssetTrackingService.DataTransferObject;
using Azure;
using Microsoft.AspNetCore.Mvc;

namespace AssetTrackingService.Controllers
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Route("[controller]")]
    public class AssetProductivityController : ControllerBase
    {
        private readonly IAssetProductivityService _assetProductivityService;

        public AssetProductivityController(IAssetProductivityService assetProductivityService)
        {
            _assetProductivityService = assetProductivityService;
        }

        [HttpGet()]
        [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
        [ProducesResponseType(typeof(List<AssetProdReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAssetProductivity()
        {
            var data = await _assetProductivityService.GetProductivityAsync();
            return Ok(data);
        }
    }
}
