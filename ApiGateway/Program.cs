using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Load Ocelot routing config ────────────────────────────────────────
// Ocelot.json defines all downstream routes for every microservice
builder.Configuration
    .AddJsonFile("Ocelot.json", optional: false, reloadOnChange: true);

// ── Register Ocelot ───────────────────────────────────────────────────
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// ── Ocelot middleware — this is the gateway, no controllers needed ────
await app.UseOcelot();

app.Run();
