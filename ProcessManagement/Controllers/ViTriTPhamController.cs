using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProcessManagement.DTOs;
using ProcessManagement.Models.KHO_TPHAM;
using ProcessManagement.Services.SQLServer;

namespace ProcessManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViTriTPhamController : ControllerBase
    {
        private readonly SQLServerServices _sqlService;
        public ViTriTPhamController(SQLServerServices sqlService)
        {
            _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
        }

        [HttpGet] // GET api/vitritpham
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ViTriTPham>>> GetViTriTPhams([FromQuery] bool isgetall = false)
        {
            var (vitritphams, error) = await _sqlService.APIGetListViTriTPhamsAsync(new Dictionary<string, object?>(), isgetall);

            if (!string.IsNullOrEmpty(error))
            {
                return Problem(error, statusCode: StatusCodes.Status400BadRequest);
            }

            return Ok(vitritphams);
        }

        [HttpGet("{id}")] // GET api/vitritpham/{id}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ViTriTPham>> GetViTriTPhamById([FromRoute] int? id)
        {
            if (id == null)
            {
                return BadRequest("ID cannot be null");
            }

            var (vitritpham, error) = await _sqlService.APIGetViTriTPhamByIdAsync(id);

            if (!string.IsNullOrEmpty(error))
            {
                return Problem(error, statusCode: StatusCodes.Status400BadRequest);
            }

            if (vitritpham == null)
            {
                return NotFound($"ViTriTPham with ID {id} not found");
            }

            return Ok(vitritpham);
        }

        // GET api/vitritpham/mapper
        [HttpGet("mapper")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<object>>> GetViTriTPhamsWithMapper([FromQuery] bool isgetall = false)
        {
            var (vitritphams, error) = await _sqlService.APIGetListViTriTPhamsAsync(new Dictionary<string, object?>(), isgetall);
            if (!string.IsNullOrEmpty(error))
            {
                return Problem(error, statusCode: StatusCodes.Status400BadRequest);
            }
            var mappedResult = vitritphams.Select(item => ViTriTPhamMapper.ToMapper(item)).ToList();
            return Ok(mappedResult);
        }

        // GET api/vitritpham/mapper/{id}
        [HttpGet("mapper/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetViTriTPhamMapperById([FromRoute] int? id)
        {
            if (id == null)
            {
                return BadRequest("ID cannot be null");
            }
            var (vitritpham, error) = await _sqlService.APIGetViTriTPhamByIdAsync(id);
            if (!string.IsNullOrEmpty(error))
            {
                return Problem(error, statusCode: StatusCodes.Status400BadRequest);
            }
            if (vitritpham == null)
            {
                return NotFound($"ViTriTPham with ID {id} not found");
            }
            var mappedResult = ViTriTPhamMapper.ToMapper(vitritpham);
            return Ok(mappedResult);
        }

        [HttpPost] // POST api/vitritpham
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> CreateViTriTPham([FromBody] ViTriTPham model)
        {
            if (model == null)
            {
                return BadRequest("Model cannot be null");
            }

            var (id, error) = await _sqlService.APIInsertViTriTPhamAsync(model);

            if (!string.IsNullOrEmpty(error))
            {
                return Problem(error, statusCode: StatusCodes.Status400BadRequest);
            }

            return CreatedAtAction(nameof(GetViTriTPhamById), new { id }, new { id });
        }

        [HttpPut] // PUT api/vitritpham
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateViTriTPham([FromBody] ViTriTPham model)
        {
            if (model == null)
            {
                return BadRequest("Model cannot be null");
            }

            var (updatedId, error) = await _sqlService.APIUpdateViTriTPhamAsync(model);

            if (!string.IsNullOrEmpty(error))
            {
                return Problem(error, statusCode: StatusCodes.Status400BadRequest);
            }

            return Ok(new { id = updatedId });
        }

        [HttpDelete("{id}")] // DELETE api/vitritpham/{id}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteViTriTPham(object id)
        {
            if (id == null)
            {
                return BadRequest("ID cannot be null");
            }

            var (success, error) = await _sqlService.APIDeleteViTriTPhamAsync(id);

            if (!string.IsNullOrEmpty(error))
            {
                return Problem(error, statusCode: StatusCodes.Status400BadRequest);
            }

            return Ok(new { success });
        }
    }
}