using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantSaaS.Api.Middleware;
using RestaurantSaaS.Application.Features.Auth.Commands;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["DATABASE_CONNECTION_STRING"] 
    ?? "Host=localhost;Port=5432;Database=restaurant_saas_dev;Username=postgres;Password=postgres;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// 2. Tenant Context & Security Services
builder.Services.AddScoped<CurrentTenantService>();
builder.Services.AddScoped<ICurrentTenantService>(sp => sp.GetRequiredService<CurrentTenantService>());
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IMfaService, MfaService>();

// 3. MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(LoginCommand).Assembly);

// 4. JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "FallbackSecretForDevelopmentPurposesMustBeAtLeast64CharactersLongSecretKey!";
var issuer = builder.Configuration["JWT_ISSUER"] ?? "RestaurantSaaS.Api";
var audience = builder.Configuration["JWT_AUDIENCE"] ?? "RestaurantSaaS.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Dev environment
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 5. Granular RBAC Authorization Policies
builder.Services.AddAuthorization(options =>
{
    var permissions = new[]
    {
        "Tenants.Manage",
        "Restaurant.Profile.Edit",
        "Branches.Manage",
        "Users.Manage",
        "Menu.Manage",
        "Orders.Create",
        "Orders.View",
        "Orders.Cancel",
        "Orders.Refund",
        "Kitchen.StatusUpdate",
        "Delivery.Dispatch",
        "Delivery.UpdateStatus",
        "CashSettlement.Submit",
        "CashSettlement.Approve",
        "Inventory.Adjust",
        "Accounting.Post",
        "Payroll.Run",
        "Reports.View"
    };

    foreach (var perm in permissions)
    {
        options.AddPolicy(perm, policy => policy.RequireClaim("permission", perm));
    }
});

// 6. Controllers & Swagger with JWT Bearer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Restaurant SaaS Modular Monolith API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTimeOffset.UtcNow }));

app.Run();
