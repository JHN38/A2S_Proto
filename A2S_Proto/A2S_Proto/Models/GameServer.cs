using System.Text.Json.Serialization;

namespace A2S_Proto.Models;

public class GameServer
{
    [JsonPropertyName("addr")]
    public string? Address { get; set; }

    [JsonPropertyName("gameport")]
    public int? GamePort { get; set; }

    [JsonPropertyName("steamid")]
    public string? SteamID { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("appid")]
    public int? AppID { get; set; }

    [JsonPropertyName("gamedir")]
    public string? GameDir { get; set; }

    [JsonPropertyName("version")]
    public Version? Version { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("region")]
    public int? Region { get; set; }

    [JsonPropertyName("players")]
    public int? Players { get; set; }

    [JsonPropertyName("max_players")]
    public int? MaxPlayers { get; set; }

    [JsonPropertyName("bots")]
    public int? Bots { get; set; }

    [JsonPropertyName("map")]
    public string? Map { get; set; }

    [JsonPropertyName("secure")]
    public bool? Secure { get; set; }

    [JsonPropertyName("dedicated")]
    public bool? Dedicated { get; set; }

    [JsonPropertyName("os")]
    public string? OperatingSystem { get; set; }

    [JsonPropertyName("gametype")]
    public string? GameType { get; set; }
}
