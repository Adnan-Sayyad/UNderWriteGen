using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<IReferralMatrixRepository, ReferralMatrixRepository>();
builder.Services.AddScoped<IReferralRepository, ReferralRepository>();

// Service registrations
builder.Services.AddScoped<IUWRuleService, UWRuleService>();
builder.Services.AddScoped<IRiskScoreService, RiskScoreService>();
builder.Services.AddScoped<IReferralMatrixService, ReferralMatrixService>();
builder.Services.AddScoped<IReferralService, ReferralService>();

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
