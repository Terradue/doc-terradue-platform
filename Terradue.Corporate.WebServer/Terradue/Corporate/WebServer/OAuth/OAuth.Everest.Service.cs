﻿using System;
using ServiceStack.ServiceHost;
using Terradue.Ldap;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using System.Web;
using Terradue.Corporate.Controller;
using ServiceStack.Common.Web;
using Terradue.Authentication.Everest;

namespace Terradue.Corporate.WebServer
{

    [Route ("/everest/cb", "GET")]
    public class OauthEverestCallBackRequest
    {
        [ApiMember (Name = "code", Description = "oauth code", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Code { get; set; }

        [ApiMember (Name = "state", Description = "oauth state", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string State { get; set; }

        [ApiMember (Name = "ajax", Description = "ajax", ParameterType = "path", DataType = "bool", IsRequired = true)]
        public bool ajax { get; set; }

        [ApiMember (Name = "error", Description = "error", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string error { get; set; }
    }

    [Route ("/oauth/everest", "GET")]
    public class OauthEverestSsoRequest
    {

        [ApiMember (Name = "return_to", Description = "return_to url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string return_to { get; set; }
    }

    [Route ("/everest/logout", "GET")]
    public class OauthEverestDeleteRequest
    {

        [ApiMember (Name = "kind", Description = "logout kind url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string kind { get; set; }

        [ApiMember (Name = "message", Description = "logout kind url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string message { get; set; }
    }

    [Api ("Terradue Corporate webserver")]
    [Restrict (EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthEverestService : ServiceStack.ServiceInterface.Service
    {
        public object Get (OauthEverestSsoRequest request)
        {
            string url;
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            try {
                context.Open ();

                context.LogInfo (this, string.Format ("/oauth/everest GET"));

                var client = new EverestOauthClient (context);
                client.AuthEndpoint = context.GetConfigValue ("everest-authEndpoint");
                client.ClientId = context.GetConfigValue ("everest-clientId");
                client.ClientSecret = context.GetConfigValue ("everest-clientSecret");
                client.TokenEndpoint = context.GetConfigValue ("everest-tokenEndpoint");
                client.Callback = context.GetConfigValue ("everest-callback");
                client.Scopes = context.GetConfigValue ("everest-scopes");

                url = client.GetAuthorizationUrl ();

                context.Close ();
            } catch (Exception e) {
                context.Close ();
                throw e;
            }

            return DoRedirect (context, url, false);
        }

        public object Get (OauthEverestCallBackRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            var redirect = "";
            UserT2 user = null;
            try {
                context.Open ();

                if (!string.IsNullOrEmpty (request.error)) {
                    context.EndSession ();
                    HttpContext.Current.Response.Redirect (context.BaseUrl, true);
                }

                var client = new EverestOauthClient (context);
                client.AccessToken (request.Code);

                EverestAuthenticationType auth = new EverestAuthenticationType (context);
                auth.SetCLient (client);

                user = (UserT2)auth.GetUserProfile (context);
                context.LogDebug (this, string.Format ("Loaded user '{0}'", user.Username));

                user.Store ();
                if (!user.HasGithubProfile ()) user.CreateGithubProfile ();
                redirect = context.GetConfigValue ("t2portal-welcomeEndpoint");

                context.Close ();
            } catch (Exception e) {
                context.Close ();
                throw e;
            }
            HttpContext.Current.Response.Redirect (redirect, true);
            return null;
        }

        public object Get (OauthEverestDeleteRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            var redirect = "";
            try {
                context.Open ();

                Connect2IdClient client = new Connect2IdClient (context.GetConfigValue ("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue ("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue ("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");

                if (!string.IsNullOrEmpty (request.kind) && request.kind.Equals ("error")) {
                    redirect = context.BaseUrl + "/portal/error?msg=Error%20from%20everest&longmsg=" + request.message;
                } else {
                    redirect = context.BaseUrl;
                }

                context.EndSession ();
                context.Close ();
            } catch (Exception e) {
                context.Close ();
                throw e;
            }
            HttpContext.Current.Response.Redirect (redirect, true);
            return null;
        }

        private HttpResult DoRedirect (IfyContext context, string redirect, bool ajax)
        {
            context.LogDebug (this, string.Format ("redirect to {0}", redirect));
            if (ajax) {
                HttpResult redirectResponse = new HttpResult ();
                redirectResponse.Headers [HttpHeaders.Location] = redirect;
                redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                return redirectResponse;
            } else {
                HttpContext.Current.Response.Redirect (redirect, true);
            }
            return null;
        }

    }
}

