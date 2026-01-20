using Microsoft.AspNetCore.Mvc;
using NetWatch.Dtos;
using NetWatch.Services;

namespace NetWatch.Controllers;


[ApiController]
[Route("devices")]
public class DevicesController : ControllerBase
{
    private readonly TopologyStorage _storage;
    public DevicesController(TopologyStorage storage)
    {
        _storage = storage;
    }


    [HttpPatch("{id}")]
    public IActionResult PatchDevice(int id, [FromBody] DevicePatchDto body)
    {
        var device = _storage.Topology.Devices.FirstOrDefault(d => d.Id == id);
        if (device == null)
        {
            return NotFound();
        }

        if (body.Active.HasValue)
        {
            device.Active = body.Active.Value;
        }
        return NoContent();
    }
}