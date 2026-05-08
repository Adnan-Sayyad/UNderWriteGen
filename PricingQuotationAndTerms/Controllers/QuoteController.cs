using Microsoft.AspNetCore.Mvc;
using PricingQuotationAndTerms.Application.DTOs.Requests;
using PricingQuotationAndTerms.Application.Interfaces;

namespace PricingQuotationAndTerms.Controllers;

[ApiController]
[Route("api/quotes")]
[Produces("application/json")]
public class QuoteController : ControllerBase
{
    private readonly IQuoteService _quoteService;
    private readonly ILogger<QuoteController> _logger;

    public QuoteController(IQuoteService quoteService, ILogger<QuoteController> logger)
    {
        _quoteService = quoteService;
        _logger       = logger;
    }

    // ═══════════════════════════════════════════════════════
    // POST /api/quotes
    // Generate a new quote — triggers the pricing engine
    // ═══════════════════════════════════════════════════════
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GenerateQuote(
        [FromBody] CreateQuoteRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var quote = await _quoteService.GenerateQuoteAsync(request, ct);
            return CreatedAtAction(nameof(GetQuote), new { quoteId = quote.QuoteId }, quote);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Submission not found.");
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule blocked quote generation.");
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════
    // GET /api/quotes/{quoteId}
    // Get a single quote by its ID
    // ═══════════════════════════════════════════════════════
    [HttpGet("{quoteId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuote(Guid quoteId, CancellationToken ct)
    {
        try
        {
            var quote = await _quoteService.GetQuoteByIdAsync(quoteId, ct);
            return quote is null
                ? NotFound(new { error = $"Quote '{quoteId}' not found." })
                : Ok(quote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error fetching quote {QuoteId}", quoteId);
            return StatusCode(500, new { error = "Failed to retrieve quote. See backend logs for details.", detail = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════
    // GET /api/quotes/submission/{submissionId}
    // Get ALL quote versions for a submission (newest first)
    // ═══════════════════════════════════════════════════════
    [HttpGet("submission/{submissionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuotesBySubmission(Guid submissionId, CancellationToken ct)
    {
        var quotes = await _quoteService.GetQuotesBySubmissionIdAsync(submissionId, ct);
        return Ok(quotes);
    }

    // ═══════════════════════════════════════════════════════
    // GET /api/quotes/submission/{submissionId}/latest
    // Get the latest quote version for a submission
    // ═══════════════════════════════════════════════════════
    [HttpGet("submission/{submissionId:guid}/latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestQuote(Guid submissionId, CancellationToken ct)
    {
        try
        {
            var quote = await _quoteService.GetLatestQuoteAsync(submissionId, ct);
            return quote is null
                ? NotFound(new { error = $"No quotes found for submission '{submissionId}'." })
                : Ok(quote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error fetching latest quote for submission {SubId}", submissionId);
            return StatusCode(500, new { error = "Failed to retrieve quote. See backend logs for details.", detail = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════
    // POST /api/quotes/{quoteId}/accept
    // Customer accepts the quote
    // ═══════════════════════════════════════════════════════
    [HttpPost("{quoteId:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AcceptQuote(Guid quoteId, CancellationToken ct)
    {
        try
        {
            await _quoteService.AcceptQuoteAsync(quoteId, ct);
            return Ok(new { message = "Quote accepted successfully.", quoteId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already accepted"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════
    // PATCH /api/quotes/{quoteId}/terms
    // Attach or update underwriter policy terms to a quote
    // ═══════════════════════════════════════════════════════
    [HttpPatch("{quoteId:guid}/terms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTerms(
        Guid quoteId,
        [FromBody] UpdateQuoteTermsRequest request,
        CancellationToken ct)
    {
        if (quoteId != request.QuoteId)
            return BadRequest(new { error = "QuoteId in URL and body must match." });

        try
        {
            await _quoteService.UpdateTermsAsync(request, ct);
            return Ok(new { message = "Terms updated successfully.", quoteId });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // ═══════════════════════════════════════════════════════
    // PATCH /api/quotes/{quoteId}/status
    // Update quote status (Draft → Presented → Declined etc.)
    // ═══════════════════════════════════════════════════════
    [HttpPatch("{quoteId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateStatus(
        Guid quoteId,
        [FromBody] UpdateQuoteStatusRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _quoteService.UpdateStatusAsync(quoteId, request, ct);
            return Ok(new { message = $"Quote status updated to '{request.Status}'.", quoteId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }
}
