using Docker.DotNet.Models;

namespace serverapi.Models
{
    public class SystemData
    {
        public float CpuPercent { get; }
        public float MemoryMb { get; }

        public SystemData(ContainerStatsResponse stats)
        {
            var cpuDelta = stats.CPUStats.CPUUsage.TotalUsage - stats.PreCPUStats.CPUUsage.TotalUsage;
            var systemDelta = stats.CPUStats.SystemUsage - stats.PreCPUStats.SystemUsage;
            var numCpus = stats.CPUStats.OnlineCPUs > 0
                ? stats.CPUStats.OnlineCPUs
                : (ulong)stats.CPUStats.CPUUsage.PercpuUsage.Count;

            CpuPercent = systemDelta == 0
                ? 0
                : (float)Math.Round((float)cpuDelta / systemDelta * numCpus * 100.0, 2);

            var cache = stats.MemoryStats.Stats.ContainsKey("cache")
                ? stats.MemoryStats.Stats["cache"]
                : 0;
            var usedMemory = stats.MemoryStats.Usage - cache;

            MemoryMb = (float)Math.Round(usedMemory / 1024.0 / 1024.0, 2);
        }
    }
}
