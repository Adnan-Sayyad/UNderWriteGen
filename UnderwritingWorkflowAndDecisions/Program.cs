using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "UW Workflow & Decisions Service", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste your JWT token here (without 'Bearer ' prefix)"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Register EF Core DbContext
builder.Services.AddDbContext<UWWorkflowDbContext>(options =>
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

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Register services
builder.Services.AddScoped<IUWNoteService, UWNoteService>();
builder.Services.AddScoped<IUWDecisionService, UWDecisionService>();
builder.Services.AddScoped<ISubjectivityService, SubjectivityService>();
builder.Services.AddScoped<IAiSummaryService, AiSummaryService>();

// Named HTTP client for Groq API (OpenAI-compatible, free tier)
builder.Services.AddHttpClient("GroqApi", c =>
{
    c.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
});

// Inter-service HTTP clients
var submissionApiUrl = builder.Configuration["Services:SubmissionApi"] ?? "http://localhost:8080/";
builder.Services.AddHttpClient<ISubmissionClientService, HttpSubmissionClientService>(c =>
    c.BaseAddress = new Uri(submissionApiUrl));

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
    var db = scope.ServiceProvider.GetRequiredService<UWWorkflowDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UW Workflow & Decisions Service v1");
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
