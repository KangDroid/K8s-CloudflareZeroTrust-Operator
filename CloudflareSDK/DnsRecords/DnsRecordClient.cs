using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudflareSDK.DnsRecords.Models.Requests;
using CloudflareSDK.DnsRecords.Models.Responses;
using CloudflareSDK.Shared.Models.Response;

namespace CloudflareSDK.DnsRecords;

public class DnsRecordClient
{
    private readonly HttpClient _httpClient;

    public DnsRecordClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Cloudflare");
    }

    /// <summary>
    ///     List Cloudflare DNS Records in a Zone
    /// </summary>
    /// <param name="zoneId">A zone Identifier</param>
    /// <param name="apiToken">API Token to authenticate client.</param>
    /// <returns>Cloudflare Response with List of DnsRecords</returns>
    /// <exception cref="NullReferenceException">When response fails.</exception>
    public async Task<CloudflareResponse<List<DnsRecordResponse>>> ListDnsRecordsAsync(string zoneId, string apiToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        var response = await _httpClient.GetAsync($"client/v4/zones/{zoneId}/dns_records");
        return await response.Content.ReadFromJsonAsync<CloudflareResponse<List<DnsRecordResponse>>>() ??
               throw new NullReferenceException("An Unknown Error Occurred while getting DNS Records");
    }

    public async Task CreateDnsRecordAsync(string zoneId, string apiToken, DnsRecordCreationRequest dnsRecordCreationRequest)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        await _httpClient.PostAsJsonAsync($"client/v4/zones/{zoneId}/dns_records", dnsRecordCreationRequest);
    }

    public async Task DeleteDnsRecordAsync(string zoneId, string dnsId, string apiToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        await _httpClient.DeleteAsync($"client/v4/zones/{zoneId}/dns_records/{dnsId}");
    }

    public async Task UpdateDnsRecordAsync(string zoneId, string dnsId, string apiToken,
                                           DnsRecordCreationRequest dnsRecordUpdateRequest)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        await _httpClient.PutAsJsonAsync($"client/v4/zones/{zoneId}/dns_records/{dnsId}", dnsRecordUpdateRequest);
    }
}