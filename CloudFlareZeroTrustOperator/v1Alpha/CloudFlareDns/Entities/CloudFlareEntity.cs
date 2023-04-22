using CloudflareSDK.DnsRecords.Models.Requests;
using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Models.Internal;
using k8s.Models;
using KubeOps.Operator.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Entities;

[KubernetesEntity(Group = "cloudflare.dns.kangdroid.dev", ApiVersion = "v1alpha", Kind = "dnsrecord",
    PluralName = "dnsrecords")]
[GenericAdditionalPrinterColumn(".spec.dnsRecordConfig.type", "Type", "string", Description = "Type of the DNS Record")]
[GenericAdditionalPrinterColumn(".spec.dnsRecordConfig.name", "DNS Record Name", "string",
    Description = "DNS Management Name(IP Address when type = a, CNAME when type = CNAME) of the DNS Record")]
[GenericAdditionalPrinterColumn(".spec.dnsRecordConfig.content", "Content", "string", Description = "DNS Content(Target)")]
public class CloudFlareEntity : CustomKubernetesEntity<CloudflareEntitySpec, CloudFlareEntity.DnsEntityStatus>
{
    public ReconcileStatus ToReconcileStatus()
    {
        if (Status.SyncLog.Count == 0)
        {
            return ReconcileStatus.NeedsCreation;
        }

        var currentTime = DateTime.UtcNow;
        var lastSynced = Status.SyncLog.MaxBy(a => a.LastSynced)!.LastSynced;
        var timeDiff = currentTime - lastSynced;

        if (timeDiff.TotalMinutes > 30 || !Equals(Spec.DnsRecordConfig, Status.LastConfiguration))
        {
            return ReconcileStatus.NeedsUpdate;
        }

        return ReconcileStatus.Skip;
    }

    public class DnsEntityStatus
    {
        public DnsRecordConfiguration LastConfiguration { get; set; }
        public List<DnsRecordSyncLog> SyncLog { get; set; } = new();
    }
}

public class CloudflareEntitySpec
{
    public CloudflareSecretReference CloudflareSecretRef { get; set; }
    public DnsRecordConfiguration DnsRecordConfig { get; set; }
}

public class DnsRecordSyncLog
{
    public DateTime LastSynced { get; set; } = DateTime.MinValue;
    public DnsRecordSyncStatus SyncStatus { get; set; }
    public string Response { get; set; } // Response from Cloudflare 
}

public class DnsRecordConfiguration
{
    /// <summary>
    ///     A valid IPv4 address.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     DNS Record name (or @ for the zone apex).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Whether the record is receiving the performance and security benefits of Cloudflare
    /// </summary>
    public bool Proxied { get; set; }

    /// <summary>
    ///     Record Type
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Comments or notes about the DNS Record. This field has no effect on DNS responses.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     Time To Live (TTL) of the DNS record in seconds. Setting to 1 means 'automatic'. Value must be between 60 and
    ///     86400, with the minimum reduced to 30 for Enterprise zones.
    /// </summary>
    public int Ttl { get; set; } = 1;

    public DnsRecordCreationRequest ToRequest()
    {
        return new DnsRecordCreationRequest
        {
            Content = Content,
            Name = Name,
            Proxied = Proxied,
            Type = Type,
            Comment = Comment,
            Ttl = Ttl
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DnsRecordConfiguration other)
        {
            return false;
        }

        return Content == other.Content
               && Name == other.Name
               && Proxied == other.Proxied
               && Type == other.Type
               && Comment == other.Comment
               && Ttl == other.Ttl;
    }
}