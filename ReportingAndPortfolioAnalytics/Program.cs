using Microsoft.EntityFrameworkCore;
using ReportingAndPortfolioAnalytics.Persistence;
using ReportingAndPortfolioAnalytics.Repositories;
using ReportingAndPortfolioAnalytics.Services;

var builder = WebApplication.CreateBuilder(args);

// ── SQL Server ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ReportingDbContext>(opts =>
	opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── App services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IHttpDataCollectorService, HttpDataCollectorService>();

// ── Scheduled background collector ───────────────────────────────────────────
builder.Services.AddHostedService<ReportCollectorScheduler>();

// ── Named HTTP clients — one per microservice ─────────────────────────────────
// Base addresses come from appsettings.json "Services" section
var services = builder.Configuration.GetSection("Services");

builder.Services.AddHttpClient("SubmissionService", c =>
	c.BaseAddress = new Uri(services["SubmissionService"]!));

builder.Services.AddHttpClient("PricingService", c =>
	c.BaseAddress = new Uri(services["PricingService"]!));

builder.Services.AddHttpClient("RulesService", c =>
	c.BaseAddress = new Uri(services["RulesService"]!));

builder.Services.AddHttpClient("PolicyService", c =>
	c.BaseAddress = new Uri(services["PolicyService"]!));

builder.Services.AddHttpClient("UWWorkflowService", c =>
	c.BaseAddress = new Uri(services["UWWorkflowService"]!));

// ── API / Swagger ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new()
{
	Title = "Reporting & Portfolio Analytics",
	Version = "v1",
	Description = "UnderWritePro — section 4.10"
}));

builder.Services.AddCors(opts =>
	opts.AddDefaultPolicy(p =>
		p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
	db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint(
		"/swagger/v1/swagger.json", "Reporting & Portfolio Analytics v1"));
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();