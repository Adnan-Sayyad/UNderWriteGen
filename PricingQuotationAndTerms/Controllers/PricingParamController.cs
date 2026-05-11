using Microsoft.AspNetCore.Mvc;
using PricingQuotationAndTerms.Application.DTOs.Requests;
using PricingQuotationAndTerms.Application.Interfaces;

namespace PricingQuotationAndTerms.Controllers;

[ApiController]
[Route("api/pricing-params")]
[Produces("application/json")]
public class PricingParamController : ControllerBase
{
    private readonly IPricingParamService _service;
    private readonly ILogger<PricingParamController> _logger;

    public PricingParamController(IPricingParamService service, ILogger<PricingParamController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    // GET /api/pricing-params
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    // POST /api/pricing-params
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePricingParamRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetAll), new { }, result);
        }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    // PUT /api/pricing-params/{paramId}
    [HttpPut("{paramId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid paramId, [FromBody] UpdatePricingParamRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            return Ok(await _service.UpdateAsync(paramId, request, ct));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // GET /api/pricing-params/product-line/{line}
    [HttpGet("product-line/{line}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByProductLine(string line, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(line))
            return BadRequest(new { error = "Product line cannot be empty." });
        return Ok(await _service.GetByProductLineAsync(line, ct));
    }

    // DELETE /api/pricing-params/{paramId}
    [HttpDelete("{paramId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid paramId, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(paramId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // GET /api/pricing-params/effective/{date}
    // Returns all params that were/are active on the given date
    // Example: /api/pricing-params/effective/2026-04-01
    [HttpGet("effective/{date}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByEffectiveDate(string date, CancellationToken ct)
    {
        if (!DateTime.TryParse(date, out var parsedDate))
            return BadRequest(new { error = $"Invalid date format '{date}'. Use yyyy-MM-dd." });

        var result = await _service.GetByEffectiveDateAsync(parsedDate.ToUniversalTime(), ct);
        return Ok(result);
    }
}
