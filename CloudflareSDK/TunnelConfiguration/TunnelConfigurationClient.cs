using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudflareSDK.Shared.Models.Response;
using CloudflareSDK.TunnelConfiguration.Models.Common;

namespace CloudflareSDK.TunnelConfiguration;

public class TunnelConfigurationClient
{
    private readonly HttpClient _httpClient;

    public TunnelConfigurationClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Cloudflare");
    }

    /// <summary>
    ///     Get Cloudflare Tunnel Configurations
    /// </summary>
    /// <param name="accountId">Account Id</param>
    /// <param name="tunnelId">Cloudflare Tunnel Id</param>
    /// <param name="apiKey">Api Key to authenticate client.</param>
    /// <returns>Cloudflare Response with Tunnel Model.</returns>
    public async Task<CloudflareResponse<TunnelModel>> GetTunnelConfigurationsAsync(
        string accountId, string tunnelId, string apiKey)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var response = await _httpClient.GetAsync($"/client/v4/accounts/{accountId}/cfd_tunnel/{tunnelId}/configurations");

        return (await response.Content.ReadFromJsonAsync<CloudflareResponse<TunnelModel>>())!;
    }

    /// <summary>
    ///     Update Cloudflare Tunnel Configurations
    /// </summary>
    /// <param name="accountId">Account Id</param>
    /// <param name="tunnelId">Cloudflare Tunnel Id</param>
    /// <param name="apiKey">API Key to authenticate client.</param>
    /// <param name="tunnelModel">A Cloudflare Tunnel Model(Tunnel Configuration)</param>
    public async Task UpdateTunnelConfigurationsAsync(string accountId, string tunnelId, string apiKey,
                                                      TunnelModel tunnelModel)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        await _httpClient.PutAsJsonAsync($"/client/v4/accounts/{accountId}/cfd_tunnel/{tunnelId}/configurations",
            tunnelModel);
    }
}