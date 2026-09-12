using RestaurantSaaS.Infrastructure.Services;

namespace RestaurantSaaS.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CurrentTenantService tenantService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            tenantService.SetFromClaims(context.User);
        }

        await _next(context);
    }
}
