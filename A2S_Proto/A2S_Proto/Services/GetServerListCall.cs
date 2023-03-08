using System.Text.Json;

namespace A2S_Proto.Services;

public class GetServerListCall
{
    private readonly HttpClient _httpClient;

    public GetServerListCall(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameServer?[]?> GetServerList(string filter, CancellationToken cancellationToken = default)
    {
        var uri = $@"https://api.steampowered.com/IGameServersService/GetServerList/v1/?key=614F7253334DA5E09186773C9AE78BF7&filter={filter}";
        var responseString = await _httpClient.GetStringAsync(uri, cancellationToken);
        var response = JsonSerializer.Deserialize<ServerListResponse>(responseString)?.Response;

        return response?.Servers;
    }
}
