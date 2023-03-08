using System.Text.Json.Serialization;

namespace A2S_Proto.Models;

public class Response
{
    [JsonPropertyName("servers")]
    public GameServer?[]? Servers { get; set; }
}

public class ServerListResponse
{
    [JsonPropertyName("response")]
    public Response? Response { get; set; }
}
