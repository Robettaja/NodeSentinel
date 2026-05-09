using System.ComponentModel.DataAnnotations;

namespace web_server.Models;

public class MinecraftServerViewModel
{
    [Required]
    public string? ServerName { get; set; } 
    public string? Motd { get; set; }
    public int MaxPlayers { get; set; }
    public MinecraftServerType ServerType { get; set; } =  MinecraftServerType.Vanilla;
    
    
    [Required]
    public string WorldName { get; set; }

    public string? Seed { get; set; } = "";
    public MinecraftMode Mode { get; set; }
    public MinecraftDifficulty Difficulty { get; set; } =  MinecraftDifficulty.Normal;
    public MinecraftLevelType WorldType { get; set; } = MinecraftLevelType.Default;
    public bool PvpMode { get; set; }


    [Required] public bool WhiteListEnabled { get; set; } = true;
    public List<string>? Whitelist { get; set; }
    
    
}

public enum MinecraftServerType
{
    Vanilla= 0,
    Paper,
    Purpur,
    Fabric,
    Forge,
    Neoforge,
    Spigot
}
public enum MinecraftDifficulty
{
    Peaceful,
    Easy,
    Normal,
    Hard,
}

public enum MinecraftMode
{
    Survival,
    Creative,
    Adventure,
    Spectator
    
}

public enum MinecraftLevelType
{
    Default,
    Flat,
    LargeBiomes,
    Amplified,
    
}