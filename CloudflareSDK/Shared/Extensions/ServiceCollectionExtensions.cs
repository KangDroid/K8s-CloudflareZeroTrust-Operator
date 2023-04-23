using CloudflareSDK.DnsRecords;
using CloudflareSDK.TunnelConfiguration;
using CloudflareSDK.Zones;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudflareSDK.Shared.Extensions;

public class DebuggableRequestHandler : DelegatingHandler
{
    private readonly ILogger<DebuggableRequestHandler> _logger;

    public DebuggableRequestHandler(ILogger<DebuggableRequestHandler> logger)
    {
        _logger = logger;
    }

    public DebuggableRequestHandler(HttpMessageHandler innerHandler, ILogger<DebuggableRequestHandler> logger) :
        base(innerHandler)
    {
        _logger = logger;
    }

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogTrace("Response: {response}", await response.Content.ReadAsStringAsync(cancellationToken));

        return response;
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCloudflareSdk(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<DebuggableRequestHandler>();
        serviceCollection.AddHttpClient("Cloudflare", (_, client) =>
        {
            client.BaseAddress = new Uri("https://api.cloudflare.com");
        }).AddHttpMessageHandler<DebuggableRequestHandler>();

        serviceCollection.AddTransient<ZoneClient>();
        serviceCollection.AddTransient<DnsRecordClient>();
        serviceCollection.AddTransient<TunnelConfigurationClient>();
        serviceCollection.AddSingleton<CloudflareClient>();

        return serviceCollection;
    }
}