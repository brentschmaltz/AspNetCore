using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JweSample
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
            // Add framework services.
            services.AddMvc();
            services.AddAuthentication(
                SharedOptions => SharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookieAuthentication();
            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                ClientId = "client-001",
                CallbackPath = Configuration["Authentication:Oidc:CallbackPath"],
                MetadataAddress = "https://testingsts.azurewebsites.net/.well-known/openid-configuration",
                Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                    OnMessageReceived = OnMessageReceived,
                    OnTokenValidated = OnTokenValidated
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            context.ProtocolMessage.SetParameter("tParms", "yJKV0VfYWxnIjogIkExMjhLVyIsICJKV0VfZW5jIjogIkExMjhDQkMtSFMyNTYiLCAiSldFX2FsZ19rZXlfa2lkIjogInN5bV9rZXlfMjU2In0");
            return Task.FromResult(0);
        }

        private Task OnMessageReceived(MessageReceivedContext context)
        {
            return Task.FromResult(0);
        }

        private Task OnTokenValidated(TokenValidatedContext context)
        {
            return Task.FromResult(0);
        }
    }
}
