using System.Text.Json.Serialization;

namespace CloudflareSDK.TunnelConfiguration.Models.Common;

public class TunnelModel
{
    [JsonPropertyName("tunnel_id")]
    public string TunnelId { get; set; }

    public TunnelConfig Config { get; set; }
}

public class TunnelConfig
{
    public List<TunnelIngress> Ingress { get; set; }
}

public class TunnelIngress
{
    public string Service { get; set; }
    public string Hostname { get; set; }
    public Dictionary<string, string> OriginRequest { get; set; }
}