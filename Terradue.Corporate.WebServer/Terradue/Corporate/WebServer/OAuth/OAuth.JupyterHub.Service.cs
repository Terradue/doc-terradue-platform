using System;
using ServiceStack.ServiceHost;
using Terradue.Ldap;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using System.Web;
using System.Security.Cryptography;
using Terradue.Corporate.Controller;
using Terradue.Authentication.Ldap;

namespace Terradue.Corporate.WebServer
{

    [Route ("/jupyterhub/cb", "GET")]
    public class OauthJupyterHubCallBackRequest
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

    [Route ("/jupyterhub/sso", "GET")]
    public class OauthJupyterHubSsoRequest
    {
        [ApiMember (Name = "sso", Description = "oauth sso", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string sso { get; set; }

        [ApiMember (Name = "sig", Description = "oauth sig", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string sig { get; set; }
    }

    [Api ("Terradue Corporate webserver")]
    [Restrict (EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthJupyterHubService : ServiceStack.ServiceInterface.Service
    {
        public object Get (OauthJupyterHubSsoRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding ();
            try {
                context.Open ();
                var client = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue ("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue ("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");

                HttpContext.Current.Session ["jupyterhub-nonce"] = nonce;

                //redirect to t2 portal SSO
                using (var service = base.ResolveService<OAuthGatewayService> ()) {
                    var response = service.Get (new OAuthAuthorizationRequest {
                        client_id = context.GetConfigValue
                            ("sso-clientId"),
                        response_type = "code",
                        nonce = nonce,
                        state = Guid.NewGuid ().ToString (),
                        redirect_uri = context.BaseUrl + "/t2api/jupyterhub/cb",
                        ajax = false
                    });
                };

                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }

            return null;
        }

        private static string HashHMAC (string key, string msg)
        {
            var encoding = new System.Text.ASCIIEncoding ();
            var bkey = encoding.GetBytes (key);
            var bmsg = encoding.GetBytes (msg);
            var hash = new HMACSHA256 (bkey);
            var hashmac = hash.ComputeHash (bmsg);
            return BitConverter.ToString (hashmac).Replace ("-", "").ToLower ();
        }

        public object Get (OauthJupyterHubCallBackRequest request)
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

                Connect2IdClient client = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue ("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue ("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");
                client.RedirectUri = context.BaseUrl + "/t2api/jupyterhub/cb";
                client.AccessToken (request.Code);

                LdapAuthenticationType auth = new LdapAuthenticationType (context);
                auth.SetConnect2IdCLient (client);

                user = (UserT2)auth.GetUserProfile (context);.
                user.LoadLdapInfo ();//TODO: should be done automatically on the previous call
                user.Store ();

                var nonce = HttpContext.Current.Session ["jupyterhub-nonce"];

                //build payload
                var payload = string.Format ("nonce={0}&email={1}&external_id={2}&username={3}&name={4}&require_activation=true",
                                         nonce,
                                         user.Email,
                                         user.Identifier,
                                         user.Username,
                                         user.Name
                                        );

                redirect = string.Format ("{0}?username.",
                                         context.GetConfigValue ("JupyterHub-sso-callback"),
                                         sso,
                                         sig);
                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            HttpContext.Current.Response.Redirect (redirect, true);
            return null;
        }
    }
}

