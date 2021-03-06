﻿using Lunch.Authorization;
using Lunch.Domain.Config;
using Lunch.Host.Filters;
using Lunch.Services.Dishes;
using Lunch.Services.People;
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
                    options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAllOrigins"));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
            

            services.Configure<GoogleConfig>(Configuration.GetSection("Google"));
            services.Configure<ProviderConfig>(Configuration.GetSection("Provider"));
            services.Configure<PeopleConfig>(Configuration.GetSection("People"));
            services.Configure<DishesConfig>(Configuration.GetSection("Dishes"));
            
            services.AddSingleton<ISheetsClient, GoogleSheetsClient>();
            services.AddTransient<IProvidersService, ProvidersService>();
            services.AddTransient<IPeopleService, PeopleService>();
            services.AddTransient<IDishesService, DishesService>();
            
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<AuthorizationFilter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            loggerFactory.AddFile("Logs/Lunch-{Date}.txt", LogLevel.Critical);

            /*app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowCredentials()
            );*/

        }
    }
}
