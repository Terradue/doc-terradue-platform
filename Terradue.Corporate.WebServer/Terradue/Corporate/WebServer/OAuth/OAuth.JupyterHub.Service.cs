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

		[ApiMember(Name = "sig", Description = "oauth sig", ParameterType = "query", DataType = "string", IsRequired = true)]
		public string sig { get; set; }

		[ApiMember(Name = "redirect_uri", Description = "oauth redirect_uri", ParameterType = "query", DataType = "string", IsRequired = true)]
		public string redirect_uri { get; set; }

		[ApiMember(Name = "client_id", Description = "oauth client_id", ParameterType = "query", DataType = "string", IsRequired = true)]
		public string client_id { get; set; }

		[ApiMember(Name = "client_secret", Description = "oauth client_secret", ParameterType = "query", DataType = "string", IsRequired = true)]
		public string client_secret { get; set; }
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
            string redirect = context.BaseUrl;
            try {
                context.Open ();

                context.LogInfo(this, string.Format("/jupyterhub/sso GET"));

				var base64Payload = System.Convert.FromBase64String(request.sso);
				var payload = encoding.GetString(base64Payload);
				var querystring = HttpUtility.ParseQueryString(payload);
				var nonce = querystring["nonce"];

				//validate the payload
				var sig = OAuthUtils.HashHMAC(context.GetConfigValue("sso-jupyterhub-clientSecret"), request.sso);
				if (!sig.Equals(request.sig)) throw new Exception("Invalid payload");

                HttpContext.Current.Session["jupyterhub-nonce"] = nonce;
                HttpContext.Current.Session["jupyterhub-callback"] = request.redirect_uri;

				redirect = string.Format("{0}?client_id={1}&response_type={2}&nonce={3}&state={4}&redirect_uri={5}&ajax={6}",
												 context.BaseUrl + "/t2api/oauth",
												 context.GetConfigValue("sso-jupyterhub-clientId"),
												 "code",
												 nonce,
												 Guid.NewGuid().ToString(),
												 context.BaseUrl + "/t2api/jupyterhub/cb",
												 "false"
												);
                
                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }

            return OAuthUtils.DoRedirect(context, redirect, false);
        }

        public object Get (OauthJupyterHubCallBackRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            var redirect = "";
            UserT2 user = null;
            try {
                context.Open ();

                context.LogInfo(this, string.Format("/jupyterhub/cb GET"));

                if (!string.IsNullOrEmpty (request.error)) {
                    context.EndSession ();
                    return OAuthUtils.DoRedirect(context, context.BaseUrl, false);
                }

                Connect2IdClient client = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue ("sso-jupyterhub-clientId");
                client.SSOApiSecret = context.GetConfigValue ("sso-jupyterhub-clientSecret");
                client.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");
                client.RedirectUri = context.BaseUrl + "/t2api/jupyterhub/cb";
                client.AccessToken (request.Code);

                LdapAuthenticationType auth = new LdapAuthenticationType (context);
                auth.SetConnect2IdCLient (client);
                user = (UserT2)auth.GetUserProfile (context);
                user.LoadLdapInfo ();//TODO: should be done automatically on the previous call
                user.Store ();

                var nonce = HttpContext.Current.Session["jupyterhub-nonce"];
                var callback = HttpContext.Current.Session["jupyterhub-callback"];

				//build payload
				var payload = string.Format("nonce={0}&email={1}&external_id={2}&username={3}&name={4}&require_activation=true",
										 nonce,
										 user.Email,
										 user.Username,
										 user.Username,
										 user.Caption != null ? user.Caption.Replace(" ", "+") : ""
										);
                

				System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
				byte[] payloadBytes = encoding.GetBytes(payload);
				var sso = System.Convert.ToBase64String(payloadBytes);
				var sig = OAuthUtils.HashHMAC(context.GetConfigValue("sso-jupyterhub-clientSecret"), sso);
				redirect = string.Format("{0}?sso={1}&sig={2}",
										 callback,
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

