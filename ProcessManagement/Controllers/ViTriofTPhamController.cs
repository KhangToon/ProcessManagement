using Microsoft.AspNetCore.Mvc;
using ProcessManagement.Models.KHO_TPHAM;
using ProcessManagement.Services.SQLServer;

[ApiController]
[Route("api/[controller]")]
public class ViTriofTPhamController : ControllerBase
{
    private readonly SQLServerServices _sqlService;

    public ViTriofTPhamController(SQLServerServices sqlService)
    {
        _sqlService = sqlService;
    }

    [HttpGet]
    public IActionResult GetList([FromQuery] bool isgetAll = false)
    {
        var (result, error) = _sqlService.GetListViTriofTPhams(new Dictionary<string, object?>(), isgetAll);
        if (!string.IsNullOrEmpty(error))
            return BadRequest(error);
        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create([FromBody] ViTriofTPham model)
    {
        var (id, error) = _sqlService.InsertViTriofTPham(model);
        if (!string.IsNullOrEmpty(error))
            return BadRequest(error);
        return Ok(new { id });
    }

    [HttpPut]
    public IActionResult Update([FromBody] ViTriofTPham model)
    {
        var (id, error) = _sqlService.UpdateViTriofTPham(model);
        if (!string.IsNullOrEmpty(error))
            return BadRequest(error);
        return Ok(new { id });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(object id)
    {
        var (success, error) = _sqlService.DeleteViTriofTPham(id);
        if (!success)
            return BadRequest(error);
        return Ok();
    }
}