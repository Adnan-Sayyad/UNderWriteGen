using System.Net;
using System.Text.Json;
using DistributionAndPartyManagement.Models.DTOs;

namespace DistributionAndPartyManagement.Middleware
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;

		public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error: {Message}", ex.Message);
				await HandleExceptionAsync(context, ex);
			}
		}

		private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var (statusCode, message) = exception switch
			{
				NotFoundException => (HttpStatusCode.NotFound, exception.Message),
				DuplicateException => (HttpStatusCode.Conflict, exception.Message),
				BusinessRuleException => (HttpStatusCode.BadRequest, exception.Message),
				ForbiddenException => (HttpStatusCode.Forbidden, exception.Message),
				_ => (HttpStatusCode.InternalServerError, "An internal server error occurred.")
			};

			context.Response.StatusCode = (int)statusCode;
			context.Response.ContentType = "application/json";

			var response = ApiResponse<object>.Fail(message);
			var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			await context.Response.WriteAsync(json);
		}
	}
}
