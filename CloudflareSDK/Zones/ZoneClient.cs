using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudflareSDK.Shared.Models.Response;
using CloudflareSDK.Zones.Models.Responses;

namespace CloudflareSDK.Zones;

public class ZoneClient
{
    private readonly HttpClient _httpClient;

    public ZoneClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Cloudflare");
    }

    /// <summary>
    ///     Get Cloudflare Zone Details
    /// </summary>
    /// <param name="zoneId">A Zone ID</param>
    /// <param name="apiToken">An API Token to authenticate Cloudflare Client</param>
    /// <returns>A Cloudflare Response with Zone Detail Response.</returns>
    public async Task<CloudflareResponse<ZoneDetailResponse>> GetZoneDetailsAsync(string zoneId, string apiToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        var response = await _httpClient.GetAsync($"client/v4/zones/{zoneId}");
        return (await response.Content.ReadFromJsonAsync<CloudflareResponse<ZoneDetailResponse>>())!;
    }
}