using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Scaleway.Functions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseScalewayFunctions(this IApplicationBuilder app, params Uri[] allowOrigins)
        {
            app.UseForwardedHeaders();
            app.Use(async (ctx, next) =>
            {
                var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Scaleway.Functions.UseScalewayFunctions");
                var scwCtx = ctx.RequestServices.GetService<ScalewayContext>();
                if (scwCtx == null)
                {
                    logger.LogError("Failed to use the Scaleway Functions middleware since the middleware doesn't seem to be registered with the service provider. Please make sure AddScalewayFunctions() has been called on the application's service collection.");
                    throw new InvalidOperationException($"Could not create instance of service {typeof(ScalewayContext).FullName}; make sure AddScalewayFunctions() has been called on the application's service collection.");
                }

                // Set the Access-Control-Allow-Origin header according to the specified allowed origins and the actual
                // origin of this request.
                if (!allowOrigins.Any())
                {
                    ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");    
                }
                else if (ctx.Request.Headers.TryGetValue("Origin", out StringValues origin))
                {
                    ctx.Response.Headers.Add("Access-Control-Allow-Origin", origin.ToString());
                }

                // Determine if we need validate authentication token.
                if (scwCtx.IsRunningAsFunction && scwCtx.RequiresAuthentication.GetValueOrDefault(false))
                {
                    // Yes. Determine if the token is valid at this point in time and that it covers this particular Scaleway namespace and application.
                    var now = DateTimeOffset.UtcNow;
                    var nbf = DateTimeOffset.FromUnixTimeSeconds(scwCtx?.DecryptedToken?.NotValidBeforeInSecondsSinceEpoch ?? 0);
                    var iat = DateTimeOffset.FromUnixTimeSeconds(scwCtx?.DecryptedToken?.IssuedAtInSecondsSinceEpoch ?? 0);
                    var exp = DateTimeOffset.FromUnixTimeSeconds(scwCtx?.DecryptedToken?.ExpiresAtInSecondsSinceEpoch ?? 0);
                    if (scwCtx?.DecryptedToken == null || nbf > now || iat > now || exp < now || ((scwCtx?.DecryptedToken?.Claims?.Length ?? 0) > 0 && !(scwCtx?.DecryptedToken?.Claims?.Any(c => string.Equals(c.NamespaceId, scwCtx?.NamespaceId, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrWhiteSpace(c.ApplicationId) || string.Equals(c.ApplicationId, scwCtx?.ApplicationId, StringComparison.InvariantCultureIgnoreCase))) ?? false)))
                    {
                        if (scwCtx?.Token != null)
                        {
                            logger.LogWarning("Validation of provided Scaleway token failed for private function {ScwApplicationName}/{ScwApplicationId} in namespace {ScwNamespaceId}.", scwCtx?.ApplicationName, scwCtx?.ApplicationId, scwCtx?.NamespaceId);
                            ctx.Response.StatusCode = 403;
                        }
                        else
                        {
                            logger.LogWarning("Scaleway token was not provided in request to private Scaleway function {ScwApplicationName}/{ScwApplicationName} in namespace {ScwNamespaceId}.", scwCtx?.ApplicationName, scwCtx?.ApplicationId, scwCtx?.NamespaceId);
                            ctx.Response.StatusCode = 401;
                        }
                        return;
                    }
                    logger.LogDebug("Request to private Scaleway function {ScwApplicationName}/{ScwApplicationId} in namespace {ScwNamespaceId} was successfully authenticated.", scwCtx?.ApplicationName, scwCtx?.ApplicationId, scwCtx?.NamespaceId);
                }
                else
                {
                    logger.LogDebug("Validation of Scaleway token is not performed since request either does not run in a Scaleway function or the function has public access.");
                }

                logger.LogDebug("Call to Scaleway function with HTTP method {HttpMethod}, path {HttpPath} and headers\n{HttpHeaders}", ctx.Request.Method, ctx.Request.Path.Value, string.Join("\n", ctx.Request.Headers.Select(h => $"{h.Key}: {h.Value}")));

                await next();
            });

            return app;
        }
    }
}