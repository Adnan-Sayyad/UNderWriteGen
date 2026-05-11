using Microsoft.EntityFrameworkCore;
using PricingQuotationAndTerms.Application.Interfaces;
using PricingQuotationAndTerms.Application.Services;
using PricingQuotationAndTerms.Contracts.Interfaces;
using PricingQuotationAndTerms.Domain.Repositories;
using PricingQuotationAndTerms.Infrastructure.BackgroundJobs;
using PricingQuotationAndTerms.Infrastructure.Data;
using PricingQuotationAndTerms.Infrastructure.ExternalApis;
using PricingQuotationAndTerms.Infrastructure.Repositories;

namespace PricingQuotationAndTerms;

public static class DependencyInjection
{
    public static IServiceCollection AddPricingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("QuoteDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<QuoteDbContext>(o =>
                o.UseInMemoryDatabase("QuoteDb_Dev"));
        }
        else
        {
            services.AddDbContext<QuoteDbContext>(o =>
                o.UseSqlServer(connectionString));
        }

        // ── Repositories ──────────────────────────────────────────────
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IPricingParamRepository, PricingParamRepository>();

        // ── Application Services ──────────────────────────────────────
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IPricingParamService, PricingParamService>();

        services.AddScoped<QuoteService>();
        services.AddScoped<IQuoteService>(sp => sp.GetRequiredService<QuoteService>());
        services.AddScoped<IQuoteApi>(sp    => sp.GetRequiredService<QuoteService>());

        // ── Public Contracts ──────────────────────────────────────────
        services.AddScoped<IPricingApi, PricingApiAdapter>();

        // ── External Services — real HTTP clients ─────────────────────
        // Calls the live microservices using base URLs from appsettings.json:
        //   Services:SubmissionApi  → http://localhost:8083/api/
        //   Services:RulesApi       → http://localhost:8085/api/
        //   Services:AgentApi       → http://localhost:8082/api/
        services.AddHttpClient<ISubmissionApi, HttpSubmissionApi>(c =>
            c.BaseAddress = new Uri(configuration["Services:SubmissionApi"]!));
        services.AddHttpClient<IRulesApi, HttpRulesApi>(c =>
            c.BaseAddress = new Uri(configuration["Services:RulesApi"]!));
        services.AddHttpClient<IAgentApi, HttpAgentApi>(c =>
            c.BaseAddress = new Uri(configuration["Services:AgentApi"]!));

        // ── Background Jobs ───────────────────────────────────────────
        services.AddHostedService<QuoteExpiryJob>();

        return services;
    }
}
