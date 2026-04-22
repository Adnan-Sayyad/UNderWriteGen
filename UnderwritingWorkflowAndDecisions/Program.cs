using Microsoft.EntityFrameworkCore;
using UnderwritingWorkflowAndDecisions.Data;
using UnderwritingWorkflowAndDecisions.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "UW Workflow & Decisions Service", Version = "v1" });
});

// Register EF Core DbContext
builder.Services.AddDbContext<UWWorkflowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IUWNoteService, UWNoteService>();
builder.Services.AddScoped<IUWDecisionService, UWDecisionService>();
builder.Services.AddScoped<ISubjectivityService, SubjectivityService>();

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

app.UseAuthorization();
app.MapControllers();
app.Run();
