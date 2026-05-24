using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.Data;
using RulesScoringAndReferralMatrix.Repositories;
using RulesScoringAndReferralMatrix.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Accept and emit enums as names ("Refer", "Active") instead of integers.
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<RulesScoringAndReferralMatrixDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository registrations
builder.Services.AddScoped<IUWRuleRepository, UWRuleRepository>();
builder.Services.AddScoped<IRiskScoreRepository, RiskScoreRepository>();
// Service registrations
builder.Services.AddScoped<IUWRuleService, UWRuleService>();
builder.Services.AddScoped<IRiskScoreService, RiskScoreService>();

// Inter-service HTTP client — calls SubmissionAndIntake to fetch risk factors
var submissionApiUrl = builder.Configuration["Services:SubmissionApi"];
if (string.IsNullOrWhiteSpace(submissionApiUrl))
{
    submissionApiUrl = "http://localhost:8083/api/";
    Console.WriteLine($"[WARN] Services:SubmissionApi not configured — defaulting to {submissionApiUrl}");
}
builder.Services.AddHttpClient<ISubmissionClientService, HttpSubmissionClientService>(c =>
    c.BaseAddress = new Uri(submissionApiUrl));

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

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Inter-service HTTP clients
var notificationApiUrl = builder.Configuration["Services:NotificationApi"];
if (string.IsNullOrWhiteSpace(notificationApiUrl))
{
    notificationApiUrl = "http://localhost:8084/";
    Console.WriteLine($"[WARN] Services:NotificationApi not configured — defaulting to {notificationApiUrl}");
}
builder.Services.AddHttpClient<INotificationClientService, HttpNotificationClientService>(c =>
    c.BaseAddress = new Uri(notificationApiUrl));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RulesScoringAndReferralMatrixDbContext>();
    db.Database.Migrate();
}

app.Run();
