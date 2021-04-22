using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
                        RSA? publicKey = null;

                        var key = Environment.GetEnvironmentVariable("SCW_PUBLIC_KEY") ?? string.Empty;
                        if (requiresAuthentication && !string.IsNullOrWhiteSpace(key))
                        {
                            try
                            {
                                publicKey = RSA.Create();
                                publicKey.ImportFromPem(key);
                            }
                            catch (CryptographicException) {}
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
                            PublicKey = publicKey
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