using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

// ---- JWT Authentication ----
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAuthorization();

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

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
