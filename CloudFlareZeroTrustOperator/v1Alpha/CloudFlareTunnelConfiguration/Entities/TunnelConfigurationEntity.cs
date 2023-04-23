using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using k8s.Models;
using KubeOps.Operator.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareTunnelConfiguration.Entities;

[KubernetesEntity(Group = "cloudflare.tunnel.kangdroid.dev", ApiVersion = "v1alpha", Kind = "tunnelconfiguration",
    PluralName = "tunnelconfigurations")]
[GenericAdditionalPrinterColumn(".status.syncStatus[0].fullDomain", "Hostname", "string",
    Description = "Public Host Subdomain")]
[GenericAdditionalPrinterColumn(".spec.tunnelConfig.service", "Destination", "string", Description = "Service to tunnel to")]
public class TunnelConfigurationEntity : CustomKubernetesEntity<TunnelConfigurationSpec,
    TunnelConfigurationEntity.TunnelConfigurationStatus>
{
    public ReconcileStatus ToReconcileStatus()
    {
        if (Status.SyncStatus.Count == 0)
        {
            return ReconcileStatus.NeedsCreation;
        }

        var currentTime = DateTime.UtcNow;
        var lastSynced = Status.SyncStatus.MaxBy(a => a.LastSynced)!.LastSynced;
        var timeDiff = currentTime - lastSynced;

        if (timeDiff.TotalMinutes > 30 || !Equals(Spec.TunnelConfig, Status.LastConfiguration))
        {
            return ReconcileStatus.NeedsUpdate;
        }

        return ReconcileStatus.Skip;
    }

    public class TunnelConfigurationStatus
    {
        public TunnelConfiguration LastConfiguration { get; set; }
        public List<TunnelSyncStatus> SyncStatus { get; set; } = new();
    }
}

public class TunnelConfigurationSpec
{
    public bool CreateDnsEntry { get; set; } = true;
    public CloudflareSecretReference CloudflareSecretRef { get; set; }
    public TunnelConfiguration TunnelConfig { get; set; }
}

public class TunnelConfiguration
{
    public string PublicHostSubdomain { get; set; }
    public string Service { get; set; }
    public string TunnelId { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not TunnelConfiguration other)
        {
            return false;
        }

        return PublicHostSubdomain == other.PublicHostSubdomain &&
               Service == other.Service;
    }
}

public class TunnelSyncStatus
{
    public DateTime LastSynced { get; set; }
    public SyncOps LastSyncOperation { get; set; }
    public string FullDomain { get; set; }
}

public enum SyncOps
{
    Created,
    Updated,
    Skipped,
    Error
}