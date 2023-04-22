using System.Text;
using CloudflareSDK;
using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Entities;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Models.Internal;
using k8s.Models;
using KubeOps.KubernetesClient;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Services;

public class CloudflareDnsService
{
    private readonly CloudflareClient _cloudflareClient;
    private readonly IKubernetesClient _kubernetesClient;
    private readonly ILogger<CloudflareDnsService> _logger;

    public CloudflareDnsService(CloudflareClient cloudflareClient, ILogger<CloudflareDnsService> cloudflareDnsService,
                                IKubernetesClient kubernetesClient)
    {
        _cloudflareClient = cloudflareClient;
        _logger = cloudflareDnsService;
        _kubernetesClient = kubernetesClient;
    }

    public async Task<DnsRecordSyncLog> AddDnsRecordIfNotExistsAsync(
        CloudFlareEntity entity, ReconcileStatus reconcileStatus)
    {
        var entitySpec = entity.Spec;
        _logger.LogInformation("Entity {0} called reconciliation, Reason: {1}", entitySpec.DnsRecordConfig.Name,
            reconcileStatus.ToString());
        var (apiKey, zoneId) = await GetApiKeyAndZoneId(entity);
        if (apiKey == null || zoneId == null)
        {
            return new DnsRecordSyncLog
            {
                LastSynced = DateTime.UtcNow,
                Response = "No Api Key or Zone Id found",
                SyncStatus = DnsRecordSyncStatus.SecretInvalid
            };
        }

        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(zoneId, apiKey);
        var dnsRecords = await _cloudflareClient.DnsRecords.ListDnsRecordsAsync(zoneId, apiKey);
        var targetDnsRecord =
            dnsRecords.Result.FirstOrDefault(a => a.Name == $"{entitySpec.DnsRecordConfig.Name}.{zoneDetails.Result.Name}");

        if (targetDnsRecord == null)
        {
            await _cloudflareClient.DnsRecords.CreateDnsRecordAsync(zoneId, apiKey,
                entitySpec.DnsRecordConfig.ToRequest());
            return new DnsRecordSyncLog
            {
                LastSynced = DateTime.UtcNow,
                Response = "",
                SyncStatus = DnsRecordSyncStatus.RecordCreated
            };
        }

        return new DnsRecordSyncLog
        {
            LastSynced = DateTime.UtcNow,
            Response = "",
            SyncStatus = DnsRecordSyncStatus.RecordAlreadySynced
        };
    }

    public async Task DeleteDnsRecordIfExists(CloudFlareEntity entity)
    {
        var spec = entity.Spec;
        _logger.LogInformation("Entity {0} called deletion", spec.DnsRecordConfig.Name);
        _logger.LogInformation("Secret Name: {0}, Namespace: {1}", spec.CloudflareSecretRef.Name, entity.Namespace());

        var (apiKey, zoneId) = await GetApiKeyAndZoneId(entity);
        if (apiKey == null || zoneId == null)
        {
            _logger.LogInformation("Operator Controller did not found Api Key or Zone Id, skipping..");
            return;
        }

        // Get Zone Details
        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(zoneId, apiKey);

        // Get DNS Records
        var response = await _cloudflareClient.DnsRecords.ListDnsRecordsAsync(zoneId, apiKey);
        var dnsRecords =
            response.Result.FirstOrDefault(a => a.Name == $"{spec.DnsRecordConfig.Name}.{zoneDetails.Result.Name}");

        if (dnsRecords != null)
        {
            _logger.LogInformation("Operator Controller found DNS Record name: {0}, Id: {1} to delete", dnsRecords.Name,
                dnsRecords.Id);
            await _cloudflareClient.DnsRecords.DeleteDnsRecordAsync(zoneId, dnsRecords.Id, apiKey);
        }
        else
        {
            _logger.LogInformation("Operator Controller did not found DNS Record name: {0}, skipping..",
                spec.DnsRecordConfig.Name);
        }
    }

    public async Task<DnsRecordSyncLog> UpdateDnsRecordAsync(CloudFlareEntity entity)
    {
        var spec = entity.Spec;
        var oldConfig = entity.Status.LastConfiguration;

        var (apiKey, zoneId) = await GetApiKeyAndZoneId(entity);
        if (apiKey == null || zoneId == null)
        {
            return new DnsRecordSyncLog
            {
                LastSynced = DateTime.UtcNow,
                Response = "No Api Key or Zone Id found",
                SyncStatus = DnsRecordSyncStatus.SecretInvalid
            };
        }

        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(zoneId, apiKey);
        var dnsRecords = await _cloudflareClient.DnsRecords.ListDnsRecordsAsync(zoneId, apiKey);
        var targetDnsRecord =
            dnsRecords.Result.FirstOrDefault(a => a.Name == $"{oldConfig.Name}.{zoneDetails.Result.Name}");
        _logger.LogInformation("DnsRecord tried to find: {0}", $"{oldConfig.Name}.{zoneDetails.Result.Name}");

        if (targetDnsRecord == null)
        {
            _logger.LogError("Cannot update dns record since it does not exist");
            return new DnsRecordSyncLog
            {
                LastSynced = DateTime.UtcNow,
                SyncStatus = DnsRecordSyncStatus.RecordNotExists,
                Response = ""
            };
        }

        await _cloudflareClient.DnsRecords.UpdateDnsRecordAsync(zoneId, targetDnsRecord.Id, apiKey,
            spec.DnsRecordConfig.ToRequest());

        return new DnsRecordSyncLog
        {
            LastSynced = DateTime.UtcNow,
            SyncStatus = DnsRecordSyncStatus.RecordUpdated,
            Response = ""
        };
    }

    private async Task<(string?, string?)> GetApiKeyAndZoneId(CloudFlareEntity entity)
    {
        var secretInfo = await _kubernetesClient.Get<V1Secret>(entity.Spec.CloudflareSecretRef.Name, entity.Namespace());

        if (secretInfo?.Data == null || !secretInfo.Data.ContainsKey("apiKey") || !secretInfo.Data.ContainsKey("zoneId"))
        {
            return (null, null);
        }

        var apiKey = Encoding.UTF8.GetString(secretInfo.Data["apiKey"]);
        var zoneId = Encoding.UTF8.GetString(secretInfo.Data["zoneId"]);
        return (apiKey, zoneId);
    }
}