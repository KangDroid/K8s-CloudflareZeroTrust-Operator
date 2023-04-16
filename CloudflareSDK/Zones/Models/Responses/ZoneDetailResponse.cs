using System.Text.Json.Serialization;

namespace CloudflareSDK.Zones.Models.Responses;

public class ZoneDetailResponse
{
    /// <summary>
    ///     The account the zone belongs to
    /// </summary>
    [JsonPropertyName("account")]
    public ZoneAccount Account { get; set; }

    /// <summary>
    ///     The last time proof of ownership was detected and the zone was made active
    /// </summary>
    [JsonPropertyName("activated_on")]
    public DateTime ActivatedOn { get; set; }

    /// <summary>
    ///     When the zone was created
    /// </summary>
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    ///     The interval (in seconds) from when development mode expires (positive integer) or last expired (negative integer)
    ///     for the domain. If development mode has never been enabled, this value is 0.
    /// </summary>
    [JsonPropertyName("development_mode")]
    public int DevelopmentMode { get; set; }

    /// <summary>
    ///     Zone Identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    ///     Metadata about the zone
    /// </summary>
    [JsonPropertyName("meta")]
    public ZoneMeta Meta { get; set; }

    /// <summary>
    ///     When the zone was last modified
    /// </summary>
    [JsonPropertyName("modified_on")]
    public DateTime ModifiedOn { get; set; }

    /// <summary>
    ///     The domain name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///     DNS host at the time of switching to Cloudflare
    /// </summary>
    [JsonPropertyName("original_dnshost")]
    public string OriginalDnsHost { get; set; }

    /// <summary>
    ///     Original name servers before moving to Cloudflare
    /// </summary>
    [JsonPropertyName("original_name_servers")]
    public List<string> OriginalNameServers { get; set; }

    /// <summary>
    ///     Registrar for the domain at the time of switching to Cloudflare
    /// </summary>
    [JsonPropertyName("original_registrar")]
    public string OriginalRegistrar { get; set; }

    /// <summary>
    ///     The owner of the zone.
    /// </summary>
    [JsonPropertyName("owner")]
    public ZoneOwner Owner { get; set; }

    /// <summary>
    ///     An array of domains used for custom name servers. This is only available for Business and Enterprise plans.
    /// </summary>
    [JsonPropertyName("vanity_name_servers")]
    public List<string> VanityNameServers { get; set; }
}

public class ZoneAccount
{
    /// <summary>
    ///     Identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name of the account
    /// </summary>
    public string Name { get; set; }
}

public class ZoneMeta
{
    /// <summary>
    ///     The zone is only configured for CDN
    /// </summary>
    [JsonPropertyName("cdn_only")]
    public bool CdnOnly { get; set; }

    /// <summary>
    ///     Number of Custom Certificates the zone can have
    /// </summary>
    [JsonPropertyName("custom_certificate_quota")]
    public int CustomCertificateQuota { get; set; }

    /// <summary>
    ///     The zone is only configured for DNS
    /// </summary>
    [JsonPropertyName("dns_only")]
    public bool DnsOnly { get; set; }

    /// <summary>
    ///     The zone is setup with Foundation DNS
    /// </summary>
    [JsonPropertyName("foundation_dns")]
    public bool FoundationDns { get; set; }

    /// <summary>
    ///     Number of Page Rules a zone can have
    /// </summary>
    [JsonPropertyName("page_rule_quota")]
    public int PageRuleQuota { get; set; }

    /// <summary>
    ///     The zone has been flagged for phisihing
    /// </summary>
    [JsonPropertyName("phishing_detected")]
    public bool PhishingDetected { get; set; }

    [JsonPropertyName("step")]
    public int Step { get; set; }
}

public class ZoneOwner
{
    /// <summary>
    ///     Identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    ///     Name of the Owner
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    ///     The type of owner
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }
}