namespace NetWatch.Dtos;

using System.Collections.Generic;

public class InitialStateDto
{
    public DeviceEventType Type { get; set; } = DeviceEventType.INITIAL_STATE;
    public List<int> DeviceIds { get; set; } = new();
}

public class DeviceEventDto
{
    public DeviceEventType Type { get; set; }
    public int DeviceId { get; set; }
}

public enum DeviceEventType
{
    INITIAL_STATE,
    REMOVED,
    ADDED,
}