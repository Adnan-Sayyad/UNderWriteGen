using Microsoft.EntityFrameworkCore;
using PricingQuotationAndTerms.Application.Interfaces;
using PricingQuotationAndTerms.Application.Services;
using PricingQuotationAndTerms.Contracts.Interfaces;
using PricingQuotationAndTerms.Domain.Repositories;
using PricingQuotationAndTerms.Infrastructure.BackgroundJobs;
using PricingQuotationAndTerms.Infrastructure.Data;
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

        // ── External Services (DEV: Fake stubs) ───────────────────────
        services.AddScoped<ISubmissionApi, FakeSubmissionApi>();
        services.AddScoped<IRulesApi,      FakeRulesApi>();
        services.AddScoped<IAgentApi,      FakeAgentApi>();

        // ── PRODUCTION: Uncomment when teammates' services are ready ──
        // services.AddHttpClient<ISubmissionApi, HttpSubmissionApi>(c =>
        //     c.BaseAddress = new Uri(configuration["Services:SubmissionApi"]!));
        // services.AddHttpClient<IRulesApi, HttpRulesApi>(c =>
        //     c.BaseAddress = new Uri(configuration["Services:RulesApi"]!));

        // ── Background Jobs ───────────────────────────────────────────
        services.AddHostedService<QuoteExpiryJob>();

        return services;
    }
}
