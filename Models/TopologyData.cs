using System.Collections.Generic;
namespace NetWatch.Models;

public class Topology
{
    public List<Device> Devices { get; set; } = new();
    public List<Connection> Connections { get; set; } = new();
}