
namespace serverapi.Models;

public record BackupItem
{
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; } = 0;
    public DateTime CreatedAt { get; set; }

}
