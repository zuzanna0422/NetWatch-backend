using NetWatch.Models;

namespace NetWatch.Services;

public class ReachableDevices
{
    private readonly TopologyStorage _storage;
    public ReachableDevices(TopologyStorage storage)
    {
        _storage = storage;
    }
    public List<int> GetReachableDevicesIds(int startId)
    {
        var results = new List<int>();
        var visited = new HashSet<int>();
        var queue = new Queue<int>();

        var startDevice = _storage.Topology.Devices.FirstOrDefault(d => d.Id == startId);

        if (startDevice == null || startDevice.Active == false)
        {
            return results.ToList();
        }

        visited.Add(startId);
        queue.Enqueue(startId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            if (!_storage.Neighbors.ContainsKey(currentId))
            {
                continue;
            }

            var neighbors = _storage.Neighbors[currentId];

            foreach (var nextId in neighbors)
            {
                if (visited.Contains(nextId))
                {
                    continue;
                }

                var nextDevice = _storage.Topology.Devices.FirstOrDefault(d => d.Id == nextId);
                if (nextDevice == null || !nextDevice.Active)
                {
                    continue;
                }

                visited.Add(nextId);
                queue.Enqueue(nextId);
                results.Add(nextId);
            }
        }

        results.Sort();
        return results;

    }
}