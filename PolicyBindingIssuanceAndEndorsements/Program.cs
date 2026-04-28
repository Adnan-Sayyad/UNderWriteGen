using Microsoft.EntityFrameworkCore;
using PolicyBindingIssuanceAndEndorsements.Data;
using PolicyBindingIssuanceAndEndorsements.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Policy Binding, Issuance & Endorsements Service", Version = "v1" });
});

// Register EF Core DbContext
builder.Services.AddDbContext<PolicyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IEndorsementService, EndorsementService>();
builder.Services.AddScoped<ICancellationService, CancellationService>();
builder.Services.AddScoped<IRenewalService, RenewalService>();

// Inter-service HTTP clients
builder.Services.AddHttpClient<ISubmissionClientService, HttpSubmissionClientService>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:SubmissionApi"]!));
builder.Services.AddHttpClient<INotificationClientService, HttpNotificationClientService>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:NotificationApi"]!));

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

app.UseAuthorization();
app.MapControllers();
app.Run();
