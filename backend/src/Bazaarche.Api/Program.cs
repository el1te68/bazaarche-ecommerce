using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Bazaarche.Api.Infrastructure;
using Bazaarche.Application;
using Bazaarche.Application.Common;
using Bazaarche.Infrastructure;
using Bazaarche.Infrastructure.Persistence;
using Bazaarche.Infrastructure.Security;
using Bazaarche.Infrastructure.Seeding;
using Bazaarche.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o =>
    {
        // Return validation errors in the same { message } shape as other errors (plus the full list).
        o.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => e.Value!.Errors.Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? "مقدار نامعتبر است." : x.ErrorMessage).ToArray());
            var first = errors.Values.SelectMany(v => v).FirstOrDefault() ?? "اطلاعات ارسالی نامعتبر است.";
            return new BadRequestObjectResult(new { message = first, errors });
        };
    });

var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters (set Jwt__Key in the environment).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = AppClaims.Name,
            RoleClaimType = AppClaims.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole("Admin"));

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddCors(o => o.AddPolicy("dev", p =>
    p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
     .AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

// ---------- Database: migrate (or create) + seed ----------
await InitializeDatabaseAsync(app);

// ---------- Pipeline ----------
app.UseMiddleware<ExceptionMiddleware>();

if (app.Configuration.GetValue("ApiDocs:Enabled", true))
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // UI at /scalar/v1
}

var storage = app.Services.GetRequiredService<LocalFileStorage>();
Directory.CreateDirectory(storage.Root);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storage.Root),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    }
});

app.UseCors("dev");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // The database container may need a few seconds to accept connections.
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            if (db.Database.GetMigrations().Any())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();
            break;
        }
        catch (Exception ex) when (attempt < 15)
        {
            logger.LogWarning("Database not ready (attempt {Attempt}): {Message}", attempt, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
}

public partial class Program;
