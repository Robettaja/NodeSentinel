using System.ComponentModel.DataAnnotations;

namespace web_server.Models;

public class ValheimServerViewModel
{
    [Required]
    public string ServerName {get; set;}

    public bool CrossPlay { get; set; } = true;
    
    [Required]
    public string WorldName {get; set;}
    
    public string Password {get; set;}
    
}