using System.Text;
using k8s.Models;
using KubeOps.KubernetesClient;

namespace CloudFlareZeroTrustOperator.Shared.Models.Internal;

public class CloudflareSecretReference
{
    public string Name { get; set; }
}

public class CloudflareSecret
{
    public string ApiKey { get; set; }
    public string AcocuntId { get; set; }
    public string ZoneId { get; set; }

    public async static Task<CloudflareSecret?> FromSecretReference(string name, string @namespace,
                                                                    IKubernetesClient kubernetesClient)
    {
        var secretInfo = await kubernetesClient.Get<V1Secret>(name, @namespace);

        if (secretInfo?.Data == null || !secretInfo.Data.ContainsKey("apiKey") || !secretInfo.Data.ContainsKey("zoneId"))
        {
            return null;
        }

        var accountId = Encoding.UTF8.GetString(secretInfo.Data["accountId"]);
        var apiKey = Encoding.UTF8.GetString(secretInfo.Data["apiKey"]);
        var zoneId = Encoding.UTF8.GetString(secretInfo.Data["zoneId"]);
        return new CloudflareSecret
        {
            AcocuntId = accountId,
            ApiKey = apiKey,
            ZoneId = zoneId
        };
    }
}