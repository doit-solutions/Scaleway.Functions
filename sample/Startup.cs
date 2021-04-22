using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scaleway.Functions
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services
                .AddResponseCompression()
                .AddProblemDetails()
                // Add the services required for Scaleway Functions hosting to function.
                .AddScalewayFunctions()
                .AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseProblemDetails()
                // Add the Scaleway Function middleware as early as possible in the pipeline since it performs authorization.
                .UseScalewayFunctions()
                .UseResponseCompression();
            if (!env.IsDevelopment())
            {
                app
                    .UseHsts()
                    .UseHttpsRedirection();
            }
            else if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app
                .UseRouting()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
