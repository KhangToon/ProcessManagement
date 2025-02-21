using Microsoft.AspNetCore.Mvc;
using ProcessManagement.Models.KHO_TPHAM;
using ProcessManagement.Services.SQLServer;

namespace ProcessManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViTriTPhamController : ControllerBase
    {
        private readonly SQLServerServices _sQLServerServices;

        public ViTriTPhamController(SQLServerServices sQLServerServices)
        {
            _sQLServerServices = sQLServerServices;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViTriTPham>>> GetViTriTPhams([FromQuery] Dictionary<string, object?> parameters, bool isgetall = false)
        {
            var (vitritphams, error) = await _sQLServerServices.APIGetListViTriTPhamsAsync(parameters, isgetall);

            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(new { message = error });
            }

            return Ok(vitritphams);
        }
    }
}
