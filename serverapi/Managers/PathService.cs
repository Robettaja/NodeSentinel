namespace serverapi.Managers;

public static class PathService
{
    private static IConfiguration _configuration;

    public static void Init(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static string GetServerPath(string? containerId = null, bool forDocker = false)
    {
        if (containerId is null)
        {
            return _configuration.GetValue<string>("HostServerPath");

        }
        if (forDocker)
        {
            var dockerPath = _configuration.GetValue<string>("HostServerPath");
            if (string.IsNullOrEmpty(dockerPath))
            {
                throw new InvalidOperationException("HostServerPath configuration is missing.");
            }
            return Path.Combine(dockerPath, containerId);
        }
        return Path.Combine(AppContext.BaseDirectory, "servers", containerId);
    }

    public static string GetBackupPath(string? containerId = null, bool forDocker = false)
    {

        if (containerId is null)
        {
            return _configuration.GetValue<string>("HostBackupPath");

        }
        if (forDocker)
        {
            var dockerPath = _configuration.GetValue<string>("HostBackupPath");
            if (string.IsNullOrEmpty(dockerPath))
            {
                throw new InvalidOperationException("HostBackupPath configuration is missing.");
            }
            return Path.Combine(dockerPath, containerId);
        }
        return Path.Combine(AppContext.BaseDirectory, "backups", containerId);
    }
}
