using System.Text.Json;
using NetWatch.Dtos;
using NetWatch.Models;

namespace NetWatch.Services;

public class TopologyStorage
{
    public Topology Topology { get; }
    public Dictionary<int, HashSet<int>> Neighbors { get; } = new();
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
                    From = c.From,
                    To = c.To
                });
            }
        }
        Topology = topology;
        BuildNeighborsDevices();
    }

    private void BuildNeighborsDevices()
    {
        foreach (var device in Topology.Devices)
        {
            Neighbors[device.Id] = new HashSet<int>();
        }

        foreach (var c in Topology.Connections)
        {
            if (!Neighbors.ContainsKey(c.From)) Neighbors[c.From] = new HashSet<int>();
            if (!Neighbors.ContainsKey(c.To)) Neighbors[c.To] = new HashSet<int>();

            Neighbors[c.From].Add(c.To);
            Neighbors[c.To].Add(c.From);
        }

    }
}