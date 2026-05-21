using System.Text;
using ComplianceAuditAndQA.Data;
using ComplianceAuditAndQA.Services;
using ComplianceAuditAndQA.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Authentication ────────────────────────────────────────────────────
var jwtKey    = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAud    = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Force the old JwtSecurityTokenHandler so token validation is compatible
        // with how IdentityAndAccessManagement signs tokens (no 'kid' header).
        // The newer JsonWebTokenHandler (default in .NET 8+) can fail key lookup
        // when neither the token nor the key has a KeyId set.
        options.UseSecurityTokenValidators = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAud,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IComplianceChecklistService, ComplianceChecklistService>();
builder.Services.AddScoped<IAuthorityBreachService,     AuthorityBreachService>();
builder.Services.AddScoped<IExceptionLogService,        ExceptionLogService>();

// Inter-service HTTP clients
builder.Services.AddHttpClient<ISubmissionClientService, HttpSubmissionClientService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:SubmissionApi"]!);
    c.Timeout     = TimeSpan.FromSeconds(6); // outer safety net (inner uses 5 s)
});

var notificationApiUrl = builder.Configuration["Services:NotificationApi"];
if (string.IsNullOrWhiteSpace(notificationApiUrl))
{
    notificationApiUrl = "http://localhost:8084/";
    Console.WriteLine($"[WARN] Services:NotificationApi not configured — defaulting to {notificationApiUrl}");
}
builder.Services.AddHttpClient<INotificationClientService, HttpNotificationClientService>(c =>
    c.BaseAddress = new Uri(notificationApiUrl));

// ── Controllers + Swagger ─────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialise response DTOs in camelCase so Angular can read
        // r.data / r.success / r.message without a PascalCase mismatch.
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        // Also deserialise case-insensitively so incoming payloads are flexible.
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Compliance, Audit & QA API",
        Version = "v1"
    });

    // Bearer token support in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Build ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
    db.Database.Migrate();
}

// ── Seed data (run with: dotnet run seed-data) ────────────────────────────
if (args.Contains("seed-data"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
    await DatabaseSeeder.SeedAsync(db);
    Console.WriteLine("[Seeder] Done. Exiting.");
    return;
}

// ── Global Exception Handler ──────────────────────────────────────────────
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async ctx =>
    {
        var feature = ctx.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (feature?.Error is not null)
        {
            var (status, message) = feature.Error switch
            {
                KeyNotFoundException        => (404, feature.Error.Message),
                InvalidOperationException   => (400, feature.Error.Message),
                UnauthorizedAccessException => (401, feature.Error.Message),
                ArgumentException           => (400, feature.Error.Message),
                _                           => (500, "An unexpected error occurred.")
            };
            // Explicitly serialise to a JSON string so Angular's HttpClient
            // always receives a parseable body regardless of content-type negotiation.
            var json = System.Text.Json.JsonSerializer.Serialize(
                new { success = false, message },
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
            ctx.Response.StatusCode  = status;
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.WriteAsync(json);
        }
    });
});

// ── Swagger ───────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Compliance, Audit & QA API v1");
    options.RoutePrefix = "swagger";
});

// ── Pipeline ──────────────────────────────────────────────────────────────
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
