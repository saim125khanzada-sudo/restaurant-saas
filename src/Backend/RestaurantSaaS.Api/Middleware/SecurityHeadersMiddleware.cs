using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RestaurantSaaS.Api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Prevent MIME-type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // 2. Clickjacking defense
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // 3. Cross-Site Scripting (XSS) filter
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // 4. Referrer Policy
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // 5. Content Security Policy (Strict default)
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data: https:; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';";

        // 6. Permissions Policy
        context.Response.Headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(self), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

        await _next(context);
    }
}
