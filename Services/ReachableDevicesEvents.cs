using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using NetWatch.Dtos;

namespace NetWatch.Services;

public class ReachableDevicesEvents
{
    private readonly ReachableDevices _reachableDevices;
    private readonly object _lock = new();
    private readonly List<Subscription> _subscriptions = new();

    public ReachableDevicesEvents(ReachableDevices reachableDevices)
    {
        _reachableDevices = reachableDevices;
    }

    public Subscription Subscribe(int rootId)
    {
        var initial = _reachableDevices.GetReachableDevicesIds(rootId);
        var sub = new Subscription(rootId, new HashSet<int>(initial));

        lock (_lock)
        {
            _subscriptions.Add(sub);
        }

        return sub;
    }

    public void Unsubscribe(Subscription sub)
    {
        lock (_lock)
        {
            _subscriptions.Remove(sub);
        }

        sub.Channel.Writer.TryComplete();
    }

    public void NotifyDeviceChange()
    {
        List<Subscription> subs;
        lock (_lock)
        {
            subs = _subscriptions.ToList();
        }

        foreach (var sub in subs)
        {
            var newList = _reachableDevices.GetReachableDevicesIds(sub.RootId);
            var newSet = new HashSet<int>(newList);

            List<int> removed;
            List<int> added;

            lock (sub.Sync)
            {
                removed = sub.LastReachable.Except(newSet).ToList();
                added = newSet.Except(sub.LastReachable).ToList();
                sub.LastReachable = newSet;
            }

            foreach (var id in removed)
            {
                sub.Channel.Writer.TryWrite(new DeviceEventDto
                {
                    Type = DeviceEventType.REMOVED,
                    DeviceId = id
                });
            }

            foreach (var id in added)
            {
                sub.Channel.Writer.TryWrite(new DeviceEventDto
                {
                    Type = DeviceEventType.ADDED,
                    DeviceId = id
                });
            }
        }
    }
}

public class Subscription
{
    public int RootId { get; }
    public Channel<DeviceEventDto> Channel { get; } = global::System.Threading.Channels.Channel.CreateUnbounded<DeviceEventDto>();

    public HashSet<int> LastReachable { get; set; }
    public object Sync { get; } = new();

    public Subscription(int rootId, HashSet<int> lastReachable)
    {
        RootId = rootId;
        LastReachable = lastReachable;
    }

    public List<int> GetSnapshotSorted()
    {
        lock (Sync)
        {
            return LastReachable.OrderBy(x => x).ToList();
        }
    }
}
