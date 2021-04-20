using System;
using Microsoft.AspNetCore.Hosting;

namespace Scaleway.Functions
{
    public static class WebHostBuiderExtensions
    {
        public static IWebHostBuilder UseScalewayFunctionsHosting(this IWebHostBuilder builder)
        {
            builder
                .UseKestrel(opts =>
                {
                    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SCW_APPLICATION_ID")) && int.TryParse(Environment.GetEnvironmentVariable("PORT"), out int port))
                    {
                        opts.ListenAnyIP(port);
                    }
                });
            
            return builder;
        }
    }
}