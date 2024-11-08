using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using System.IO;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

[assembly: OwinStartup(typeof(ElectronicPartsProject.Startup))]

namespace ElectronicPartsProject
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cargar configuración desde el archivo JSON
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            var jsonConfig = JObject.Parse(File.ReadAllText(path));
            var clientId = jsonConfig["AzureAd"]["ClientId"].ToString();
            var tenantId = jsonConfig["AzureAd"]["TenantId"].ToString();
            var clientSecret = jsonConfig["AzureAd"]["ClientSecret"].ToString();
            var redirectUri = jsonConfig["AzureAd"]["RedirectUri"].ToString();

            // Mostrar PII en el entorno de desarrollo (para depuración)
            IdentityModelEventSource.ShowPII = true;

            // Configuración de autenticación basada en cookies (esto debe estar primero)
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                SlidingExpiration = true
            });

            // Configuración de OpenIdConnect para Azure AD
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = $"https://login.microsoftonline.com/" + tenantId,
                RedirectUri = redirectUri,
                ClientSecret = clientSecret,
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                PostLogoutRedirectUri = "https://web.osela.com/PortalAppsTest/",
                Scope = "openid profile email User.Read",
                SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType, // Especificar el tipo de autenticación
                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = clientId, // O el valor de "Audience" en tu app registration si es diferente
                    ValidIssuer = $"https://login.microsoftonline.com/" + tenantId + "/v2.0"
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = context =>
                    {
                        // Forzar la autenticación si el usuario no está autenticado
                        if (!context.OwinContext.Authentication.User.Identity.IsAuthenticated)
                        {
                            context.OwinContext.Authentication.Challenge(
                                //TODO: Change dev or prod
                                //new AuthenticationProperties { RedirectUri = "https://localhost:44323" },
                                new AuthenticationProperties { RedirectUri = "https://web.osela.com/ElectroParts" },
                                OpenIdConnectAuthenticationDefaults.AuthenticationType);
                        }
                        return Task.FromResult(0);
                        //context.ProtocolMessage.Prompt = "login"; // Fuerza autenticación cada vez
                        //return Task.FromResult(0);
                    },
                    AuthenticationFailed = context =>
                    {
                        System.Diagnostics.Trace.TraceError("Authentication failed: " + context.Exception);
                        context.HandleResponse();
                        context.Response.Redirect("/Error?message=" + context.Exception.Message);
                        return Task.FromResult(0);
                    },
                    SecurityTokenValidated = context =>
                    {
                        System.Diagnostics.Trace.TraceInformation("Token validated: " + context.AuthenticationTicket.Identity.Name);
                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}
