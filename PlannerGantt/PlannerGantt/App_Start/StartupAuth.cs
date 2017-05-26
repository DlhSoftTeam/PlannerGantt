using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Web.SessionState;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace PlannerGantt
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientID"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string authority = aadInstance + "common";
        private static string applicationUri = ConfigurationManager.AppSettings["ApplicationUri"];
        private static string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            string authorizationCode = null;
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false
                    },
                    RedirectUri = applicationUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {   
                        SecurityTokenValidated = (context) =>
                        {
                            return Task.FromResult(0);
                        },
                        AuthenticationFailed = (context) =>
                        {
                            context.HandleResponse();
                            return Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = (context) =>
                        {
                            authorizationCode = context.Code;
                            return Task.FromResult(0);
                        }
                    }
                });
            app.UseStageMarker(PipelineStage.Authenticate);
            app.Use((context, next) =>
            {
                var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                var session = httpContext.Session;
                if (session != null && session["AccessToken"] == null && authorizationCode != null)
                {
                    var authContext = new AuthenticationContext(authority);
                    var authResult = authContext.AcquireTokenByAuthorizationCodeAsync(
                        authorizationCode, 
                        new Uri(applicationUri, UriKind.Absolute),
                        new ClientCredential(clientId, clientSecret),
                        "https://graph.microsoft.com").Result;
                    session["AccessToken"] = authResult.AccessToken;
                    session.Timeout = 60;
                    authorizationCode = null;
                }
                return next();
            });
        }
    }
}
