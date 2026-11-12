var builder = WebApplication.CreateBuilder(args);

var proxyConfig = builder.Configuration.GetSection("ReverseProxy");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(proxyConfig);

var app = builder.Build();

app.MapReverseProxy(proxy =>
{
    proxy.Use(async (context, next) =>
    {
        var route = context.GetReverseProxyFeature().Route.Config;

        if (route.Metadata?.TryGetValue("Blocked", out var blocked) == true && blocked == "true")
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return; // never forwarded
        }

        await next();
    });

    proxy.UseSessionAffinity();
    proxy.UseLoadBalancing();
    proxy.UsePassiveHealthChecks();
});
app.Run();