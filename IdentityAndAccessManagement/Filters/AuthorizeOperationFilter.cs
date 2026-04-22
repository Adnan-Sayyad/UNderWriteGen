using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IdentityAndAccessManagement.Filters
{
    /// <summary>
    /// Adds Bearer security requirement to every [Authorize] endpoint.
    /// This makes Swagger UI show the 🔒 padlock and automatically send
    /// the Authorization header when the user has globally authorized.
    /// </summary>
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // ── Check method-level [Authorize] ────────────────────
            var hasAuthorize = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any();

            var hasAllowAnonymous = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>()
                .Any();

            // ── Also check controller-level [Authorize] ────────────
            if (!hasAuthorize)
            {
                hasAuthorize = context.MethodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any() ?? false;
            }

            // ── Skip non-protected or explicitly anonymous endpoints ──
            if (!hasAuthorize || hasAllowAnonymous)
                return;

            // ── Add 401 response ──────────────────────────────────
            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized — valid Bearer token required."
                });

            // ── Attach Bearer security requirement ────────────────
            // This correctly serializes as {"Bearer": []} in swagger.json
            // so Swagger UI knows to send the Authorization header.
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
    }
}
