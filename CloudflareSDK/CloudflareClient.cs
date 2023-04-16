using CloudflareSDK.DnsRecords;
using CloudflareSDK.Zones;
using Microsoft.Extensions.DependencyInjection;

namespace CloudflareSDK;

public class CloudflareClient
{
    private readonly IServiceProvider _serviceProvider;
    public ZoneClient Zones => _serviceProvider.GetRequiredService<ZoneClient>();
    public DnsRecordClient DnsRecords => _serviceProvider.GetRequiredService<DnsRecordClient>();

    public CloudflareClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
}