using Microsoft.EntityFrameworkCore;
using RulesScoringAndReferralMatrix.Contracts.RepositoryContracts;
using RulesScoringAndReferralMatrix.Contracts.ServiceContracts;
using RulesScoringAndReferralMatrix.Data;
using RulesScoringAndReferralMatrix.Repositories;
using RulesScoringAndReferralMatrix.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
builder.Services.AddHttpClient<INotificationClientService, HttpNotificationClientService>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:NotificationApi"]!));

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
