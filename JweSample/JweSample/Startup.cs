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
using Microsoft.IdentityModel.Tokens;

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
                Authority = "https://testingsts.azurewebsites.net",
                ClientId = "client-001",
                CallbackPath =  "/signin-oidc",
                Events = new OpenIdConnectEvents
                {
                    OnAuthenticationFailed = OnAuthenticationFailed,
                    OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
                    OnMessageReceived = OnMessageReceived,
                    OnTokenValidated = OnTokenValidated
                },
                // user will need to set keys for decryption here
                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    TokenDecryptionKey = new SymmetricSecurityKey(Convert.FromBase64String("WIVds2iwJPwNhgUgwZXmn/46Ql1EkiL+M+QqDRdQURE="))
                    {
                        KeyId = "sym_key_256"
                    }
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error");
            return Task.FromResult(0);
        }

        private Task OnMessageReceived(MessageReceivedContext context)
        {
            return Task.FromResult(0);
        }

        private Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            // instructions to TestingSts to create a JWE using key above and 'dir' alg.
            var param = Base64UrlEncoder.Encode(@"{""JWE_alg"":""dir"",""JWE_enc"":""A128CBC-HS256"",""JWE_alg_key_kid"":""sym_key_256""}");
            context.ProtocolMessage.SetParameter("tParams", param);
            return Task.FromResult(0);
        }

        private Task OnTokenValidated(TokenValidatedContext context)
        {
            return Task.FromResult(0);
        }
    }
}
