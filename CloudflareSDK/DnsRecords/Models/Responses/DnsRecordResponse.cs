using System.Text.Json.Serialization;

namespace CloudflareSDK.DnsRecords.Models.Responses;

public class DnsRecordResponse
{
    /// <summary>
    ///     A valid IPv4 address.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     DNS Record Name(or @ for the zone apex).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Whether the record is receiving the performance and security benefits of Cloudflare
    /// </summary>
    public bool Proxied { get; set; }

    /// <summary>
    ///     Record Type(A, CNAME)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Comments or notes about the DNS record. This field has no effect on DNS Responses.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     When the record was created.
    /// </summary>
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    ///     Identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Whether this record can be modified/deleted (true means it is managed by Cloudflare)
    /// </summary>
    public bool Locked { get; set; }

    /// <summary>
    ///     When the record was last modified
    /// </summary>
    [JsonPropertyName("modified_on")]
    public DateTime ModifiedOn { get; set; }

    /// <summary>
    ///     Whether the record can be proxied by cloudflare or not.
    /// </summary>
    public bool Proxiable { get; set; }

    /// <summary>
    ///     Time To Live (TTL) of the DNS record in seconds. Setting to 1 means 'automatic'. Value must be between 60 and
    ///     86400, with the minimum reduced to 30 for Enterprise zones.
    /// </summary>
    public int Ttl { get; set; }

    /// <summary>
    ///     Zone Identifier
    /// </summary>
    [JsonPropertyName("zone_id")]
    public string ZoneId { get; set; }

    /// <summary>
    ///     The domain of the record(i.e example.com)
    /// </summary>
    [JsonPropertyName("zone_name")]
    public string ZoneName { get; set; }
}