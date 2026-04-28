using Microsoft.EntityFrameworkCore;
using PricingQuotationAndTerms.Domain.Entities;
using PricingQuotationAndTerms.Domain.Repositories;
using PricingQuotationAndTerms.Infrastructure.Data;

namespace PricingQuotationAndTerms.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IQuoteRepository.
/// Lives in Infrastructure — Domain has zero dependency on EF Core.
/// </summary>
public class QuoteRepository : IQuoteRepository
{
    private readonly QuoteDbContext _context;

    public QuoteRepository(QuoteDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task AddAsync(Quote quote, CancellationToken ct = default)
    {
        await _context.Quotes.AddAsync(quote, ct);
        await _context.SaveChangesAsync(ct);
    }

    // READ by QuoteId
    public async Task<Quote?> GetByIdAsync(Guid quoteId, CancellationToken ct = default)
    {
        return await _context.Quotes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == quoteId, ct);
    }

    // READ all quotes for a submission (multiple versions)
    public async Task<IEnumerable<Quote>> GetBySubmissionIdAsync(Guid submissionId, CancellationToken ct = default)
    {
        return await _context.Quotes
            .AsNoTracking()
            .Where(q => q.SubmissionId == submissionId)
            .OrderByDescending(q => q.VersionNo)
            .ToListAsync(ct);
    }

    // UPDATE
    public async Task UpdateAsync(Quote quote, CancellationToken ct = default)
    {
        // Re-attach because GetByIdAsync used AsNoTracking
        _context.Quotes.Update(quote);
        await _context.SaveChangesAsync(ct);
    }

    // Get the latest version number for a submission (for versioning re-quotes)
    public async Task<int> GetLatestVersionAsync(Guid submissionId, CancellationToken ct = default)
    {
        var latest = await _context.Quotes
            .AsNoTracking()
            .Where(q => q.SubmissionId == submissionId)
            .MaxAsync(q => (int?)q.VersionNo, ct);

        return latest ?? 0; // 0 means no quotes yet → new quote will be version 1
    }
}
