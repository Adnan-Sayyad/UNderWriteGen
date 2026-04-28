using Microsoft.EntityFrameworkCore;
using RiskDataAndEvidence.Persistence;
using RiskDataAndEvidence.Repositories;
using RiskDataAndEvidence.Services;

var builder = WebApplication.CreateBuilder(args);

// ── SQL Server ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<RiskDataDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── App services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IRiskProfileRepository, RiskProfileRepository>();
builder.Services.AddScoped<IEvidenceRefRepository, EvidenceRefRepository>();
builder.Services.AddScoped<IRiskProfileService, RiskProfileService>();
builder.Services.AddScoped<IEvidenceRefService, EvidenceRefService>();

// ── API / Swagger ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new()
{
    Title = "Risk Data & Evidence",
    Version = "v1",
    Description = "UnderWritePro — section 4.4"
}));

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RiskDataDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint(
        "/swagger/v1/swagger.json", "Risk Data & Evidence v1"));
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
