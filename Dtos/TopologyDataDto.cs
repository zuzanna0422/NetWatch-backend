namespace NetWatch.Dtos;

public class TopologyDataDto
{
    public List<DeviceDto> Devices { get; set; } = new();
    public List<ConnectionDto> Connections { get; set; } = new();
}