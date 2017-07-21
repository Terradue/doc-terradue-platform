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

    [Route ("/discourse/cb", "GET")]
    public class OauthDiscourseCallBackRequest
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

    [Route ("/discourse/sso", "GET")]
    public class OauthDiscourseSsoRequest
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
    public class OAuthDiscourseService : ServiceStack.ServiceInterface.Service
    {
        public object Get (OauthDiscourseSsoRequest request)
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

                var base64Payload = System.Convert.FromBase64String (request.sso);
                var payload = encoding.GetString (base64Payload);
                var querystring = HttpUtility.ParseQueryString (payload);
                var nonce = querystring ["nonce"];

                //validate the payload
                var sig = OAuthUtils.HashHMAC (context.GetConfigValue ("discourse-sso-secret"), request.sso);
                if (!sig.Equals (request.sig)) throw new Exception ("Invalid payload");

                HttpContext.Current.Session ["discourse-nonce"] = nonce;

                //redirect to t2 portal SSO
                using (var service = base.ResolveService<OAuthGatewayService> ()) {
                    var response = service.Get (new OAuthAuthorizationRequest {
                        client_id = context.GetConfigValue
                            ("sso-clientId"),
                        response_type = "code",
                        nonce = nonce,
                        state = Guid.NewGuid ().ToString (),
                        redirect_uri = context.BaseUrl + "/t2api/discourse/cb",
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

        public object Get (OauthDiscourseCallBackRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            var redirect = "";
            UserT2 user = null;
            try {
                context.Open ();

                if (!string.IsNullOrEmpty (request.error)) {
                    context.EndSession ();
                    return OAuthUtils.DoRedirect(context, context.BaseUrl, false);
                }

                Connect2IdClient client = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue ("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue ("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");
                client.RedirectUri = context.BaseUrl + "/t2api/discourse/cb";
                client.AccessToken (request.Code);

                LdapAuthenticationType auth = new LdapAuthenticationType (context);
                auth.SetConnect2IdCLient (client);

                user = (UserT2)auth.GetUserProfile (context);
                user.LoadLdapInfo ();//TODO: should be done automatically on the previous call
                user.Store ();

                var nonce = HttpContext.Current.Session ["discourse-nonce"];

                //build payload
                var payload = string.Format ("nonce={0}&email={1}&external_id={2}&username={3}&name={4}&require_activation=true",
                                         nonce,
                                         user.Email,
                                         user.Identifier,
                                         user.Username,
                                         user.Name
                                        );

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding ();
                byte [] payloadBytes = encoding.GetBytes (payload);
                var sso = System.Convert.ToBase64String (payloadBytes);
                var sig = OAuthUtils.HashHMAC (context.GetConfigValue ("discourse-sso-secret"), sso);
                redirect = string.Format ("{0}?sso={1}&sig={2}",
                                         context.GetConfigValue ("discourse-sso-callback"),
                                         sso,
                                         sig);
                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return OAuthUtils.DoRedirect(context, redirect, false);
        }
    }
}

