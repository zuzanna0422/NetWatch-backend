using System.Text.Json;
using System.Text.Json.Serialization;
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
    public IActionResult PatchDevice(int id, [FromBody] DevicePatchDto body, [FromServices] ReachableDevicesEvents eventsService)
    {
        var device = _storage.Topology.Devices.FirstOrDefault(d => d.Id == id);
        if (device == null)
        {
            return NotFound();
        }

        var changed = false;

        if (body.Active.HasValue && device.Active != body.Active.Value)
        {
            device.Active = body.Active.Value;
            changed = true;
        }
        if (changed)
        {
            eventsService.NotifyDeviceChange();
        }
        return NoContent();
    }
    [HttpGet("{id}/reachable-devices")]
    public async Task GetReachable(int id, [FromServices] ReachableDevicesEvents eventsService, CancellationToken ct)
    {
        var device = _storage.Topology.Devices.FirstOrDefault(d => d.Id == id);
        if (device == null)
        {
            Response.StatusCode = 404;
            return;
        }

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var subscription = eventsService.Subscribe(id);

        try
        {
            var initial = new InitialStateDto
            {
                DeviceIds = subscription.GetSnapshotSorted()
            };

            await WriteSseAsync(initial, jsonOptions, ct);

            await foreach (var evt in subscription.Channel.Reader.ReadAllAsync(ct))
            {
                await WriteSseAsync(evt, jsonOptions, ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            eventsService.Unsubscribe(subscription);
        }
    }
    private async Task WriteSseAsync(object payload, JsonSerializerOptions jsonOptions, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, jsonOptions);
        await Response.WriteAsync($"data: {json}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}