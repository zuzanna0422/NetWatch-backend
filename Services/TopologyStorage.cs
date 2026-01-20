using System.Text.Json;
using NetWatch.Dtos;
using NetWatch.Models;

namespace NetWatch.Services;

public class TopologyStorage
{
    public Topology Topology { get; }
    public TopologyStorage(IWebHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "Data", "topology.json");
        var json = File.ReadAllText(filePath);

        var dto = JsonSerializer.Deserialize<TopologyDataDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        var topology = new Topology();

        if (dto != null)
        {
            foreach (var d in dto.Devices)
            {
                topology.Devices.Add(new Device
                {
                    Id = d.Id,
                    Name = d.Name,
                    Active = d.Active
                });
            }

            foreach (var c in dto.Connections)
            {
                topology.Connections.Add(new Connection
                {
                    From = c.from,
                    To = c.to
                });
            }
        }
        Topology = topology;
    }
}