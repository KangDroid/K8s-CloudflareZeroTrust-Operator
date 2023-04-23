using CloudflareSDK;
using CloudflareSDK.TunnelConfiguration.Models.Common;
using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Entities;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareTunnelConfiguration.Entities;
using k8s.Models;
using KubeOps.KubernetesClient;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareTunnelConfiguration.Services;

public class TunnelConfigurationService
{
    private readonly CloudflareClient _cloudflareClient;
    private readonly IKubernetesClient _kubernetesClient;

    public TunnelConfigurationService(IKubernetesClient kubernetesClient, CloudflareClient cloudflareClient)
    {
        _kubernetesClient = kubernetesClient;
        _cloudflareClient = cloudflareClient;
    }

    public async Task CreateTunnelConfigurationAsync(TunnelConfigurationEntity entity)
    {
        var cloudflareSecret =
            await CloudflareSecret.FromSecretReference(entity.Spec.CloudflareSecretRef.Name, entity.Namespace(),
                _kubernetesClient);
        if (cloudflareSecret == null)
        {
            entity.Status.SyncStatus = new List<TunnelSyncStatus>
            {
                new()
                {
                    FullDomain = "",
                    LastSynced = DateTime.UtcNow,
                    LastSyncOperation = SyncOps.Error
                }
            };
            await _kubernetesClient.UpdateStatus(entity);
            return;
        }

        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(cloudflareSecret.ZoneId, cloudflareSecret.ApiKey);
        var tunnelConfigurations =
            await _cloudflareClient.TunnelConfiguration.GetTunnelConfigurationsAsync(cloudflareSecret.AcocuntId,
                entity.Spec.TunnelConfig.TunnelId,
                cloudflareSecret.ApiKey);

        var existingIngressConfiguration = tunnelConfigurations.Result.Config.Ingress.FirstOrDefault(a =>
            a.Hostname == $"{entity.Spec.TunnelConfig.PublicHostSubdomain}.{zoneDetails.Result.Name}");
        if (existingIngressConfiguration != null)
        {
            entity.Status.SyncStatus = new List<TunnelSyncStatus>
            {
                new()
                {
                    FullDomain = "",
                    LastSynced = DateTime.UtcNow,
                    LastSyncOperation = SyncOps.Error
                }
            };
            await _kubernetesClient.UpdateStatus(entity);
            return;
        }

        tunnelConfigurations.Result.Config.Ingress.Insert(0, new TunnelIngress
        {
            Hostname = $"{entity.Spec.TunnelConfig.PublicHostSubdomain}.{zoneDetails.Result.Name}",
            Service = entity.Spec.TunnelConfig.Service,
            OriginRequest = new Dictionary<string, string>()
        });
        await _cloudflareClient.TunnelConfiguration.UpdateTunnelConfigurationsAsync(cloudflareSecret.AcocuntId,
            entity.Spec.TunnelConfig.TunnelId, cloudflareSecret.ApiKey, tunnelConfigurations.Result);
        entity.Status.SyncStatus = new List<TunnelSyncStatus>
        {
            new()
            {
                LastSynced = DateTime.UtcNow,
                FullDomain = $"{entity.Spec.TunnelConfig.PublicHostSubdomain}.{zoneDetails.Result.Name}",
                LastSyncOperation = SyncOps.Created
            }
        };

        if (entity.Spec.CreateDnsEntry)
        {
            await CreateDnsEntryAsync(entity);
        }

        entity.Status.LastConfiguration = entity.Spec.TunnelConfig;
        await _kubernetesClient.UpdateStatus(entity);
    }

    public async Task UpdateTunnelConfigurationAsync(TunnelConfigurationEntity entity)
    {
        var cloudflareSecret =
            await CloudflareSecret.FromSecretReference(entity.Spec.CloudflareSecretRef.Name, entity.Namespace(),
                _kubernetesClient);
        if (cloudflareSecret == null)
        {
            entity.Status.SyncStatus = new List<TunnelSyncStatus>
            {
                new()
                {
                    FullDomain = "",
                    LastSynced = DateTime.UtcNow,
                    LastSyncOperation = SyncOps.Error
                }
            };
            await _kubernetesClient.UpdateStatus(entity);
            return;
        }

        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(cloudflareSecret.ZoneId, cloudflareSecret.ApiKey);
        var tunnelConfigurations =
            await _cloudflareClient.TunnelConfiguration.GetTunnelConfigurationsAsync(cloudflareSecret.AcocuntId,
                entity.Spec.TunnelConfig.TunnelId,
                cloudflareSecret.ApiKey);

        var existingIngressConfiguration = tunnelConfigurations.Result.Config.Ingress.FirstOrDefault(a =>
            a.Hostname == $"{entity.Status.LastConfiguration.PublicHostSubdomain}.{zoneDetails.Result.Name}");
        if (existingIngressConfiguration == null)
        {
            entity.Status.SyncStatus = new List<TunnelSyncStatus>
            {
                new()
                {
                    FullDomain = "",
                    LastSynced = DateTime.UtcNow,
                    LastSyncOperation = SyncOps.Error
                }
            };
            await _kubernetesClient.UpdateStatus(entity);
            return;
        }

        tunnelConfigurations.Result.Config.Ingress.Remove(existingIngressConfiguration);
        tunnelConfigurations.Result.Config.Ingress.Insert(0, new TunnelIngress
        {
            Hostname = $"{entity.Spec.TunnelConfig.PublicHostSubdomain}.{zoneDetails.Result.Name}",
            Service = entity.Spec.TunnelConfig.Service,
            OriginRequest = new Dictionary<string, string>()
        });
        await _cloudflareClient.TunnelConfiguration.UpdateTunnelConfigurationsAsync(cloudflareSecret.AcocuntId,
            entity.Spec.TunnelConfig.TunnelId, cloudflareSecret.ApiKey, tunnelConfigurations.Result);
        entity.Status.SyncStatus = new List<TunnelSyncStatus>
        {
            new()
            {
                LastSynced = DateTime.UtcNow,
                FullDomain = $"{entity.Spec.TunnelConfig.PublicHostSubdomain}.{zoneDetails.Result.Name}",
                LastSyncOperation = SyncOps.Created
            }
        };
        entity.Status.LastConfiguration = entity.Spec.TunnelConfig;
        await _kubernetesClient.UpdateStatus(entity);
    }

    public async Task DeleteTunnelConfigurationAsync(TunnelConfigurationEntity entity)
    {
        var cloudflareSecret =
            await CloudflareSecret.FromSecretReference(entity.Spec.CloudflareSecretRef.Name, entity.Namespace(),
                _kubernetesClient);
        if (cloudflareSecret == null)
        {
            return;
        }

        var zoneDetails = await _cloudflareClient.Zones.GetZoneDetailsAsync(cloudflareSecret.ZoneId, cloudflareSecret.ApiKey);
        var tunnelConfigurations =
            await _cloudflareClient.TunnelConfiguration.GetTunnelConfigurationsAsync(cloudflareSecret.AcocuntId,
                entity.Spec.TunnelConfig.TunnelId,
                cloudflareSecret.ApiKey);

        var existingIngressConfiguration = tunnelConfigurations.Result.Config.Ingress.FirstOrDefault(a =>
            a.Hostname == $"{entity.Spec.TunnelConfig.PublicHostSubdomain}.{zoneDetails.Result.Name}");
        if (existingIngressConfiguration == null)
        {
            return;
        }

        tunnelConfigurations.Result.Config.Ingress.Remove(existingIngressConfiguration);
        await _cloudflareClient.TunnelConfiguration.UpdateTunnelConfigurationsAsync(cloudflareSecret.AcocuntId,
            entity.Spec.TunnelConfig.TunnelId, cloudflareSecret.ApiKey, tunnelConfigurations.Result);

        if (entity.Spec.CreateDnsEntry)
        {
            await _kubernetesClient.Delete<CloudFlareEntity>(entity.Name(), entity.Namespace());
        }
    }

    private async Task CreateDnsEntryAsync(TunnelConfigurationEntity entity)
    {
        await _kubernetesClient.Create(new CloudFlareEntity
        {
            ApiVersion = "cloudflare.dns.kangdroid.dev/v1alpha",
            Kind = "dnsrecord",
            Metadata = new V1ObjectMeta
            {
                Name = entity.Name(),
                NamespaceProperty = entity.Namespace()
            },
            Spec = new CloudflareEntitySpec
            {
                CloudflareSecretRef = entity.Spec.CloudflareSecretRef,
                DnsRecordConfig = new DnsRecordConfiguration
                {
                    Name = entity.Spec.TunnelConfig.PublicHostSubdomain,
                    Content = $"{entity.Spec.TunnelConfig.TunnelId}.cfargotunnel.com",
                    Proxied = true,
                    Ttl = 1,
                    Type = "CNAME",
                    Comment = "Cloudflare Operator Auto Generated"
                }
            }
        });
    }
}