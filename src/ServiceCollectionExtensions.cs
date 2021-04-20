using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jose;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace Scaleway.Functions
{
    public static class ServiceCollectionExtensions
    {
        public static readonly Regex PublicKeyRegex = new Regex("^-----BEGIN RSA PUBLIC KEY-----(.+?)-----END RSA PUBLIC KEY-----$");
        
        public static IServiceCollection AddScalewayFunctions(this IServiceCollection services)
        {
            services
                .Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                })
                .AddScoped<ScalewayContext>(services =>
                {
                    var applicationId = Environment.GetEnvironmentVariable("SCW_APPLICATION_ID");
                    if (applicationId != null)
                    {
                        var requiresAuthentication = !bool.TryParse(Environment.GetEnvironmentVariable("SCW_PUBLIC"), out bool publicAccess) || !publicAccess;
                        var token = Environment.GetEnvironmentVariable("SCW_FUNCTIONS_TOKEN");
                        ScalewayToken? decryptedToken = null;
                        var tokenPublicKey = new byte[1000];
                        if (requiresAuthentication && token != null && Convert.TryFromBase64String(PublicKeyRegex.Match(Environment.GetEnvironmentVariable("SCW_PUBLIC_KEY") ?? string.Empty)?.Groups[1]?.Value?.Trim() ?? string.Empty, tokenPublicKey, out int length))
                        {
                            try
                            {
                                var rsa = RSA.Create();
                                rsa.ImportRSAPublicKey(new Span<byte>(tokenPublicKey).Slice(0, length).ToArray(), out length);
                                decryptedToken = JsonSerializer.Deserialize<ScalewayToken>(JWT.Decode(token, rsa, JwsAlgorithm.RS256));
                            }
                            catch (CryptographicException) {}
                            catch (JoseException) {}
                            catch (JsonException) {}
                        }
                        var ctx = new ScalewayContext
                        {
                            IsRunningAsFunction = applicationId != null,
                            HostName = Environment.GetEnvironmentVariable("HOSTNAME"),
                            NamespaceId = Environment.GetEnvironmentVariable("SCW_NAMESPACE_ID"),
                            ApplicationId = applicationId,
                            ApplicationName = Environment.GetEnvironmentVariable("SCW_APPLICATION_NAME"),
                            ApplicationMemoryInMb = int.TryParse(Environment.GetEnvironmentVariable("SCW_APPLICATION_MEMORY"), out int applicationMemoryInMb) ? applicationMemoryInMb : null,
                            RequiresAuthentication = requiresAuthentication,
                            Token = token,
                            DecryptedToken = decryptedToken
                        };

                        return ctx;
                    }
                    else
                    {
                        return new ScalewayContext
                        {
                            IsRunningAsFunction = false
                        };
                    }
                });
            
            
            return services;
        }
    }
}