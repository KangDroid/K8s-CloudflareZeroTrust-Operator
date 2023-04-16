namespace CloudflareSDK.DnsRecords.Models.Requests;

public class DnsRecordCreationRequest
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
}