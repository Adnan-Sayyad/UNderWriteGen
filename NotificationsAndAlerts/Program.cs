using Microsoft.EntityFrameworkCore;
using NotificationsAndAlerts.Data;
using NotificationsAndAlerts.Middleware;
using NotificationsAndAlerts.Services;
using NotificationsAndAlerts.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---- Controllers & Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Database ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- DI: Service layer ----
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// ---- Middleware pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
