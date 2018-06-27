using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MicroCoinApi.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NJsonSchema;
using NSwag.AspNetCore;

namespace MicroCoinApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSignalR();
            services.AddSwagger();
            services.AddCors(options =>
            {
                options.AddPolicy("AnyOrigin", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseCors("AnyOrigin");
            app.UseSignalR(routes =>
            {
                routes.MapHub<MicroCoinHub>("/stream");
            });
            DefaultFilesOptions defoptions = new DefaultFilesOptions();
            defoptions.DefaultFileNames.Clear();
            defoptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defoptions);
            app.UseStaticFiles();
            app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {
                    new AccountNumberConverter(),
                    new ByteStringConverter(),
                    new ECKeyPairConverter()
                }
            };
            app.UseSwaggerReDocWithApiExplorer((s) =>
            {                
                s.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;               
                s.SwaggerUiRoute = "/doc";
                s.UseJsonEditor = true;                
                s.DocExpansion = "list";                
                s.GeneratorSettings.DefaultEnumHandling = EnumHandling.String;                
                s.GeneratorSettings.Title = "MicroCoin";
                s.GeneratorSettings.Version = "1.0.0";
                s.GeneratorSettings.Description = Properties.Resources.documentation;
            });
            app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly, settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;                
                settings.UseJsonEditor = true;
                settings.SwaggerUiRoute = "/explorer";
                settings.UseJsonEditor = true;
                settings.DocExpansion = "list";
                settings.GeneratorSettings.DefaultEnumHandling = EnumHandling.String;
                settings.GeneratorSettings.Title = "MicroCoin";
                settings.GeneratorSettings.Version = "1.0.0";
                settings.GeneratorSettings.Description = Properties.Resources.documentation;
            });  /*
            app.UseSwaggerUi3WithApiExplorer((settings) =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling =
                    PropertyNameHandling.CamelCase;
            });*/
            app.UseMvc();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
    }
}
