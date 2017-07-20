using System;
using ServiceStack.ServiceHost;
using Terradue.Ldap;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using System.Web;
using Terradue.Corporate.Controller;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Terradue.Corporate.WebServer
{

    [Route ("/eosso/cb", "GET")]
    public class OauthEossoCallBackRequest
    {
        [ApiMember (Name = "payload", Description = "oauth payload", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string payload { get; set; }

		[ApiMember(Name = "sig", Description = "oauth sig", ParameterType = "query", DataType = "string", IsRequired = true)]
		public string sig { get; set; }
    }

    [Route ("/oauth/eosso", "GET")]
    public class OauthEoSsoRequest
    {
        [ApiMember (Name = "return_to", Description = "return_to url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string return_to { get; set; }
    }

    [Api ("Terradue Corporate webserver")]
    [Restrict (EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthEossoService : ServiceStack.ServiceInterface.Service
    {
        public object Get (OauthEoSsoRequest request)
        {
            string redirect;
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            try {
                context.Open ();

                context.LogInfo (this, string.Format ("/oauth/eosso GET"));

				var nonce = Guid.NewGuid().ToString();
				HttpContext.Current.Session["eosso-nonce"] = nonce;
				var callback = context.BaseUrl + "/eosso/cb";

                //build payload
                var payload = string.Format("nonce={0}&redirect_url={1}", nonce, callback);
				System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
				byte[] payloadBytes = encoding.GetBytes(payload);
				var sso = System.Convert.ToBase64String(payloadBytes);
				var sig = HashHMAC(context.GetConfigValue("sso-eosso-secret"), sso);

				redirect = string.Format("{0}?payload={1}&sig={2}",
										 context.GetConfigValue("sso-eosso-endpoint"),
										 sso,
										 sig);

                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }

            return OAuthUtils.DoRedirect (context, redirect, false);
        }

		private static string HashHMAC(string key, string msg) {
			var encoding = new System.Text.ASCIIEncoding();
			var bkey = encoding.GetBytes(key);
			var bmsg = encoding.GetBytes(msg);
			var hash = new HMACSHA256(bkey);
			var hashmac = hash.ComputeHash(bmsg);
			return BitConverter.ToString(hashmac).Replace("-", "").ToLower();
		}

        public object Get (OauthEossoCallBackRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            var redirect = "";
            UserT2 user = null;
            try {
                context.Open ();

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

				//validate payload
				var base64Payload = System.Convert.FromBase64String(request.payload);
				var payload = encoding.GetString(base64Payload);
				var querystring = HttpUtility.ParseQueryString(payload);
				var nonce = querystring["nonce"];
                var username = querystring["username"];
                var email = querystring["email"];

				//validate the payload
				var sig = HashHMAC(context.GetConfigValue("sso-eosso-secret"), request.payload);
				if (!sig.Equals(request.sig)) throw new Exception("Invalid payload");
                if (!nonce.Equals(HttpContext.Current.Session["eosso-nonce"])) throw new Exception("Invalid nonce");

                var auth = new EossoAuthenticationType (context);
                auth.SetUserInformation(username, email);

                user = (UserT2)auth.GetUserProfile (context);
                if (user == null) throw new Exception ("Error to load user");
                context.LogDebug (this, string.Format ("Loaded user '{0}'", user.Username));

                context.StartSession (auth, user);
                context.SetUserInformation (auth, user);

                //Create the session also on SSO
                var clientSSO = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
                clientSSO.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                clientSSO.SSOApiClient = context.GetConfigValue ("sso-clientId");
                clientSSO.SSOApiSecret = context.GetConfigValue ("sso-clientSecret");
                clientSSO.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");
                clientSSO.SSODirectAuthEndpoint = context.GetConfigValue ("sso-directAuthEndpoint");

                var defaultscopes = new List<string> (context.GetConfigValue ("sso-scopes").Split (",".ToCharArray ()));

                var span = DateTime.Now.Subtract (new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

                var directAuthRequest = new OauthDirectAuthzRequest {
                    client_id = clientSSO.SSOApiClient,
                    sub_session = new OauthSubSessionRequest {
                        sub = user.Username,
                        auth_time = (long)span.TotalSeconds,
                        creation_time = (long)span.TotalSeconds
                    },
                    refresh_token = new OauthRefreshToken {
                        issue = true,
                        lifetime = 3600
                    },
                    long_lived = true,
                    scope = defaultscopes
                };

                //create the SID (bypassing user credentials with direct authz
                var directAuthResponse = clientSSO.DirectAuthorization (directAuthRequest);
                var sid = directAuthResponse.sub_sid;
                var accesstoken = directAuthResponse.access_token;
                var refreshtoken = directAuthResponse.refresh_token;
                if (string.IsNullOrEmpty (sid)) throw new Exception ("SID received is empty");
                clientSSO.StoreSUBSID(sid);
                if (!string.IsNullOrEmpty (accesstoken)) {
                    clientSSO.StoreTokenAccess (accesstoken, directAuthResponse.expires_in);
                }
                if (!string.IsNullOrEmpty (refreshtoken)) {
                    clientSSO.StoreTokenRefresh (refreshtoken);
                }

                redirect = context.GetConfigValue ("t2portal-welcomeEndpoint");

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

