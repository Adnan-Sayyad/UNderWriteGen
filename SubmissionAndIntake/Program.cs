using Microsoft.EntityFrameworkCore;
using SubmissionAndIntake.Contracts.RepositoryContracts;
using SubmissionAndIntake.Contracts.ServiceContracts;
using SubmissionAndIntake.Data;
using SubmissionAndIntake.Repositories;
using SubmissionAndIntake.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<SubmissionAndIntakeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository registrations
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<ICompletenessCheckRepository, CompletenessCheckRepository>();

// Service registrations
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IQuestionnaireService, QuestionnaireService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<ICompletenessCheckService, CompletenessCheckService>();

// Inter-service HTTP clients
builder.Services.AddHttpClient<IDistributionValidationService, HttpDistributionValidationService>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:DistributionApi"]!));

// Fall back to the default port if Services:NotificationApi is not configured —
// avoids a hard startup crash when appsettings drifts and lets the operator see a clear warning.
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
