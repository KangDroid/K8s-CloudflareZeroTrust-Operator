using CloudflareSDK.DnsRecords.Models.Requests;
using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Models.Internal;
using k8s.Models;
using KubeOps.Operator.Entities;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Entities;

[KubernetesEntity(Group = "cloudflare.dns.kangdroid.dev", ApiVersion = "v1alpha", Kind = "cloudflarednsrecord",
    PluralName = "cloudflarednsrecords")]
public class CloudFlareEntity : CustomKubernetesEntity<CloudflareEntitySpec, List<CloudFlareEntity.CloudflareDnsEntityStatus>>
{
    public ReconcileStatus ToReconcileStatus()
    {
        if (Status.Count == 0)
        {
            return ReconcileStatus.NeedsCreation;
        }

        var currentTime = DateTime.UtcNow;
        var lastSynced = Status.MaxBy(a => a.LastSynced)!.LastSynced;
        var timeDiff = currentTime - lastSynced;

        if (timeDiff.TotalMinutes > 30)
        {
            return ReconcileStatus.NeedsUpdate;
        }

        return ReconcileStatus.Skip;
    }

    public class CloudflareDnsEntityStatus
    {
        public DateTime LastSynced { get; set; } = DateTime.MinValue;
        public DnsRecordSyncStatus SyncStatus { get; set; }
        public string Response { get; set; } // Response from Cloudflare
    }
}

public class CloudflareEntitySpec
{
    public string AccountId { get; set; }
    public string Zone { get; set; }
    public string ApiKey { get; set; }

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
}