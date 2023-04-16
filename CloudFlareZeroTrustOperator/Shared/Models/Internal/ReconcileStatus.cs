namespace CloudFlareZeroTrustOperator.Shared.Models.Internal;

public enum ReconcileStatus
{
    NeedsCreation,
    NeedsUpdate,
    Skip
}