using CloudflareSDK.Shared.Extensions;
using CloudFlareZeroTrustOperator.v1Alpha.CloudFlareDns.Services;
using KubeOps.Operator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCloudflareSdk();
builder.Services.AddTransient<CloudflareDnsService>();
builder.Services.AddKubernetesOperator(config =>
{
    config.HttpPort = 5000;
    config.HttpsPort = 5001;
});

var app = builder.Build();
app.UseKubernetesOperator();
await app.RunOperatorAsync(args);