using Lunch.Authorization;
using Lunch.Domain.Config;
using Lunch.Host.Filters;
using Lunch.Services.Providers;
using Lunch.Sheets.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IAuthorizationService = Lunch.Authorization.IAuthorizationService;

namespace Lunch.Host.Config
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options
                .AddPolicy("AllowAllOrigins", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowCredentials().AllowAnyHeader()));
            
            // Add framework services.
            services.AddMvc(options =>
                {
                    options.Filters.Add(typeof(AuthorizationFilter));
                    options.Filters.Add(typeof(ExceptionFilter));
                    options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAllOrigins"));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
            

            services.Configure<GoogleConfig>(Configuration.GetSection("Google"));
            services.Configure<ProviderConfig>(Configuration.GetSection("Provider"));
            
            services.AddSingleton<ISheetsClient, GoogleSheetsClient>();
            services.AddTransient<IProvidersService, ProvidersService>();
            
            services.AddScoped<IAuthorizationService, AuthorizationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            /*app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowCredentials()
            );*/

        }
    }
}
