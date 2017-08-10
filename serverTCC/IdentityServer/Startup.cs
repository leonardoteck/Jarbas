﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.Utils;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer
{
    public class Startup
    {

        public IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

       
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            /*services.AddIdentityServer()
                .AddSigningCredential(new X509Certificate2("cert.pfx"))
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddAspNetIdentity<Usuario>();*/
            //.AddExtensionGrantValidator<GoogleGrantValidator>();

            //Cfg Entity Framework + PostgreSQL
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<IdentityContext>(
                    options => options.UseNpgsql(
                        Configuration.GetConnectionString("JarbasBD")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }
    }
}
