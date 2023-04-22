using CloudflareSDK;
using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Entities;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Models.Internal;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Services;

public class CloudflareDnsService
{
    private readonly CloudflareClient _cloudflareClient;
    private readonly ILogger<CloudflareDnsService> _logger;

    public CloudflareDnsService(CloudflareClient cloudflareClient, ILogger<CloudflareDnsService> cloudflareDnsService)
    {
        _cloudflareClient = cloudflareClient;
        _logger = cloudflareDnsService;
    }

    public async Task<CloudFlareEntity.CloudflareDnsEntityStatus> AddDnsRecordIfNotExistsAsync(
        CloudflareEntitySpec entitySpec, ReconcileStatus reconcileStatus)
    {
        _logger.LogInformation("Entity {0} called reconciliation, Reason: {1}", entitySpec.Name, reconcileStatus.ToString());
        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(entitySpec.Zone, entitySpec.ApiKey);
        var dnsRecords = await _cloudflareClient.DnsRecords.ListDnsRecordsAsync(entitySpec.Zone, entitySpec.ApiKey);
        var targetDnsRecord = dnsRecords.Result.FirstOrDefault(a => a.Name == $"{entitySpec.Name}.${zoneDetails.Result.Name}");

        if (targetDnsRecord == null)
        {
            await _cloudflareClient.DnsRecords.CreateDnsRecordAsync(entitySpec.Zone, entitySpec.ApiKey,
                entitySpec.ToRequest());
            return new CloudFlareEntity.CloudflareDnsEntityStatus
            {
                LastSynced = DateTime.UtcNow,
                Response = "",
                SyncStatus = DnsRecordSyncStatus.RecordCreated
            };
        }

        return new CloudFlareEntity.CloudflareDnsEntityStatus
        {
            LastSynced = DateTime.UtcNow,
            Response = "",
            SyncStatus = DnsRecordSyncStatus.RecordAlreadySynced
        };
    }

    public async Task DeleteDnsRecordIfExists(CloudflareEntitySpec spec)
    {
        _logger.LogInformation("Entity {0} called deletion", spec.Name);

        // Get Zone Details
        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(spec.Zone, spec.ApiKey);

        // Get DNS Records
        var response = await _cloudflareClient.DnsRecords.ListDnsRecordsAsync(spec.Zone, spec.ApiKey);
        var dnsRecords = response.Result.FirstOrDefault(a => a.Name == $"{spec.Name}.{zoneDetails.Result.Name}");

        if (dnsRecords != null)
        {
            _logger.LogInformation("Operator Controller found DNS Record name: {0}, Id: {1} to delete", dnsRecords.Name,
                dnsRecords.Id);
            await _cloudflareClient.DnsRecords.DeleteDnsRecordAsync(spec.Zone, dnsRecords.Id, spec.ApiKey);
        }
        else
        {
            _logger.LogInformation("Operator Controller did not found DNS Record name: {0}, skipping..", spec.Name);
        }
    }
}