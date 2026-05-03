using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using server.Models.Tables;
using web_server.Managers;

namespace web_server.Models
{
    public class TmodServerViewModel
    {

        [Required(ErrorMessage = "This field is required")]
        [DisplayName("Server name")]
        public string ServerName { get; set; } = string.Empty;
        [DisplayName("Message of the day")]
        public string? MOTD { get; set; }
        [DisplayName("Max players")]
        public int MaxPlayers { get; set; } = 8;
        public TerrariaLanguage Language { get; set; } = TerrariaLanguage.English;

        [Required(ErrorMessage = "Worldname is required")]
        public string WorldName { get; set; } = string.Empty;
        public string? Seed { get; set; }
        [DisplayName("World size")]
        public TerrariaWorldsize WorldSize { get; set; } = TerrariaWorldsize.Medium;
        public TerrariaDifficulty Difficulty { get; set; } = TerrariaDifficulty.Expert;

        [DisplayName("Server password")]
        public string? Password { get; set; }

        public List<TerrariaMod> Mods { get; set; } = [];
        public List<string> SelectedMods { get; set; } = [];
        public bool HasMoreMods { get; set; }
        public int ModPage { get; set; } = 1;

        public string ToTerrariaLanguageCode(TerrariaLanguage language) => language switch
        {
            TerrariaLanguage.English => "en-US",
            TerrariaLanguage.German => "de-DE",
            TerrariaLanguage.Italian => "it-IT",
            TerrariaLanguage.French => "fr-FR",
            TerrariaLanguage.Spanish => "es-ES",
            TerrariaLanguage.Russian => "ru-RU",
            TerrariaLanguage.ChineseSimplified => "zh-Hans",
            TerrariaLanguage.PortugueseBrazil => "pt-BR",
            TerrariaLanguage.Polish => "pl-PL",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };

        public async Task LoadMods()
        {
            int pageSize = 20;
            Mods = await DatabaseManipulator.GetPaged<TerrariaMod>(
                _ => true, 1, pageSize, m => m.Views, ascending: false) ?? [];
            HasMoreMods = Mods.Count == pageSize;
            ModPage = 1;
        }
    }

}
