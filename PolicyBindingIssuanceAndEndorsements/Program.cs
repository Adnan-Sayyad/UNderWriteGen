using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Policy Binding, Issuance & Endorsements Service", Version = "v1" });
});

// Register EF Core DbContext
builder.Services.AddDbContext<PolicyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IEndorsementService, EndorsementService>();
builder.Services.AddScoped<ICancellationService, CancellationService>();
builder.Services.AddScoped<IRenewalService, RenewalService>();

// Inter-service HTTP clients
builder.Services.AddHttpClient<ISubmissionClientService, HttpSubmissionClientService>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:SubmissionApi"]!));

var complianceApiUrl = builder.Configuration["Services:ComplianceApi"] ?? "http://localhost:8089/";
builder.Services.AddHttpClient<IComplianceClientService, HttpComplianceClientService>(c =>
    c.BaseAddress = new Uri(complianceApiUrl));
var notificationApiUrl = builder.Configuration["Services:NotificationApi"];
if (string.IsNullOrWhiteSpace(notificationApiUrl))
{
    notificationApiUrl = "http://localhost:8084/";
    Console.WriteLine($"[WARN] Services:NotificationApi not configured — defaulting to {notificationApiUrl}");
}
builder.Services.AddHttpClient<INotificationClientService, HttpNotificationClientService>(c =>
    c.BaseAddress = new Uri(notificationApiUrl));

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Policy Binding, Issuance & Endorsements Service v1");
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
