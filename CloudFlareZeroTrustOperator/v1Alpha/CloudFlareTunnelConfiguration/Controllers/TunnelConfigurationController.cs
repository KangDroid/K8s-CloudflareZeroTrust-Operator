using CloudFlareZeroTrustOperator.Shared.Models.Internal;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareTunnelConfiguration.Entities;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareTunnelConfiguration.Services;
using k8s.Models;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;
using Newtonsoft.Json;

namespace CloudFlareZeroTrustOperator.v1Alpha.CloudFlareTunnelConfiguration.Controllers;

[EntityRbac(typeof(TunnelConfigurationEntity), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1StatefulSet), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Deployment), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Service), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Secret), Verbs = RbacVerb.All)]
public class TunnelConfigurationController : IResourceController<TunnelConfigurationEntity>
{
    private readonly ILogger<TunnelConfigurationController> _logger;
    private readonly TunnelConfigurationService _tunnelConfigurationService;

    public TunnelConfigurationController(TunnelConfigurationService tunnelConfigurationService,
                                         ILogger<TunnelConfigurationController> logger)
    {
        _tunnelConfigurationService = tunnelConfigurationService;
        _logger = logger;
    }

    public async Task<ResourceControllerResult?> ReconcileAsync(TunnelConfigurationEntity entity)
    {
        var reconcileStatus = entity.ToReconcileStatus();

        switch (reconcileStatus)
        {
            case ReconcileStatus.NeedsCreation:
                await _tunnelConfigurationService.CreateTunnelConfigurationAsync(entity);
                break;
            case ReconcileStatus.NeedsUpdate:
                await _tunnelConfigurationService.UpdateTunnelConfigurationAsync(entity);
                break;
            case ReconcileStatus.Skip:
                break;
        }

        return null;
    }

    public Task StatusModifiedAsync(TunnelConfigurationEntity entity)
    {
        _logger.LogInformation("Status modification called: {0}", JsonConvert.SerializeObject(entity));
        return Task.CompletedTask;
    }

    public async Task DeletedAsync(TunnelConfigurationEntity entity)
    {
        await _tunnelConfigurationService.DeleteTunnelConfigurationAsync(entity);
    }
}