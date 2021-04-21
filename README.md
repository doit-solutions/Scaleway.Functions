# Scaleway.Functions
[![NuGet](https://img.shields.io/nuget/v/Scaleway.Functions.svg?style=flat)](https://www.nuget.org/packages/Scaleway.Functions/)

A simple ASP.NET Core middleware for running ASP.NET Core APIs in Scaleway serverless containers.

## Getting started
This middleware has two primary functions. First, it makes sure you Kestrel instance is listening on the correct port (according to the `${PORT}` environment variable provided by Scaleway's serverless runtime). Second, it performs token validation/authorization for private functions based on the provided token and public key.

It also has some additional benefits such as providing easy access to information about the Scaleway environment.

Using this middleware requires a couple of additions to your code. First, the host builder must be modified(typically in your `Program.cs` file), adding a call to `UseScalewayFunctions()`. A simple `Program.cs` might look like

```cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Scaleway.Functions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        // Make sure Kestrel listens on the correct port.
                        .UseScalewayFunctionsHosting()
                        .UseStartup<Startup>();
                });
    }
}
```

Second, your `Startup` class must be amended, adding required services to your service provider and adding the actual middleware to the ASP.NET Core pipeline. The required services are added with a call to `AddScalewayServices()` and the middleware is added to the pipeline with a call to `UseScalewayServices()`. Since the middleware performs authroziation, it is essential that the middleware is added as early as possible in the pipeline. A sample `Startup` class might look like

```cs
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
                // Add the services required by the Scaleway.Functions middleware.
                .AddScalewayFunctions()
                .AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app
                .UseProblemDetails()
                // Add the Scaleway.Functions middleware as early as possible in the pipeline.
                .UseScalewayFunctions(new Uri("https://sample-bucket-website.s3-website.nl-ams.scw.cloud/"))
                .UseResponseCompression()
                .UseHsts()
                .UseHttpsRedirection();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app
                .UseRouting()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
```

As you can see in the sample above, it is possible to add one or more URIs, which will be added to the `Access-Control-Allow-Origin` header in the function's responses. This might come in handy if you use the function as an API backend for a Scaleway bucket website, for example.
