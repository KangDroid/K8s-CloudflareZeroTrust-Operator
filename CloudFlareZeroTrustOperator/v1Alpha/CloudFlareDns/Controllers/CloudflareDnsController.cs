using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Entities;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Services;
using k8s.Models;
using KubeOps.KubernetesClient;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Controllers;

[EntityRbac(typeof(CloudFlareEntity), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1StatefulSet), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Deployment), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Service), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Secret), Verbs = RbacVerb.All)]
public class CloudflareDnsController : IResourceController<CloudFlareEntity>
{
    private readonly CloudflareDnsService _cloudflareDnsService;
    private readonly IKubernetesClient _kubernetesClient;
    private readonly ILogger<CloudflareDnsController> _logger;

    public CloudflareDnsController(ILogger<CloudflareDnsController> logger,
                                   IKubernetesClient kubernetesClient, CloudflareDnsService cloudflareDnsService)
    {
        _logger = logger;
        _kubernetesClient = kubernetesClient;
        _cloudflareDnsService = cloudflareDnsService;
    }

    public async Task<ResourceControllerResult?> ReconcileAsync(CloudFlareEntity entity)
    {
        var reconcileStatus = entity.ToReconcileStatus();

        if (reconcileStatus == ReconcileStatus.Skip)
        {
            return null;
        }

        entity.Status.Add(await _cloudflareDnsService.AddDnsRecordIfNotExistsAsync(entity.Spec, reconcileStatus));
        await _kubernetesClient.UpdateStatus(entity);

        return null;
    }

    public Task StatusModifiedAsync(CloudFlareEntity entity)
    {
        // Still Create it
        _logger.LogInformation("Entity {0} called status modification", entity.Name());
        return Task.CompletedTask;
    }

    public async Task DeletedAsync(CloudFlareEntity entity)
    {
        await _cloudflareDnsService.DeleteDnsRecordIfExists(entity.Spec);
    }
}