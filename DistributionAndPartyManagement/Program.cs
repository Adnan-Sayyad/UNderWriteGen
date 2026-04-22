//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();
//app.UseSwagger();
//app.UseSwaggerUI();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();




using Microsoft.EntityFrameworkCore;
using DistributionAndPartyManagement.Data;
using DistributionAndPartyManagement.Middleware;
using DistributionAndPartyManagement.Repositories;
using DistributionAndPartyManagement.Repositories.Interfaces;
using DistributionAndPartyManagement.Services;
using DistributionAndPartyManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---- Controllers & Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Database ----
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Dependency Injection: Repository layer ----
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<ICustomerPartyRepository, CustomerPartyRepository>();

// ---- Dependency Injection: Service layer ----
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<ICustomerPartyService, CustomerPartyService>();

var app = builder.Build();

// ---- Middleware pipeline ----
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Global exception handling middleware (catches all errors)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

