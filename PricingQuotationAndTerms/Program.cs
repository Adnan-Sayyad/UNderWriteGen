using PricingQuotationAndTerms;
using PricingQuotationAndTerms.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Register all module services ─────────────────────────────────────
builder.Services.AddPricingModule(builder.Configuration);

// ── API Infrastructure ────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title       = "UnderwritePro — Pricing, Quotation & Terms",
        Version     = "v1",
        Description = "Member 5 module: Generate quotes, calculate premium, accept quotes."
    });
});

var app = builder.Build();

// ── Auto-create DB tables on startup ─────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
    db.Database.EnsureCreated();
}

// ── Global exception handler — returns JSON for all unhandled errors ──
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async ctx =>
    {
        ctx.Response.StatusCode  = 500;
        ctx.Response.ContentType = "application/json";
        var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var msg     = feature?.Error?.Message ?? "An unexpected error occurred.";
        await ctx.Response.WriteAsJsonAsync(new { error = msg });
    });
});

// ── Swagger — always enabled (dev machine) ────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pricing & Quotation v1");
    c.RoutePrefix = string.Empty; // Opens at http://localhost:8086/
});

// ── Pipeline ──────────────────────────────────────────────────────────
app.UseAuthorization();
app.MapControllers();

app.Run();
