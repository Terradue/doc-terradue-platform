using System;
using ServiceStack.ServiceHost;
using Terradue.Ldap;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using System.Web;
using Terradue.Corporate.Controller;
using ServiceStack.Common.Web;
using System.Collections.Generic;

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
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }

            return OAuthUtils.DoRedirect (context, url, false);
        }

        public object Get (OauthEverestCallBackRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            var redirect = "";
            UserT2 user = null;
            try {
                context.Open ();
                context.LogInfo(this, string.Format("/everest/cb GET"));

                if (!string.IsNullOrEmpty (request.error)) {
                    context.EndSession ();
                    return OAuthUtils.DoRedirect(context, context.BaseUrl, false);
                }

                var client = new EverestOauthClient (context);
                client.AccessToken (request.Code);

                EverestAuthenticationType auth = new EverestAuthenticationType (context);
                auth.SetCLient (client);

				try {
					user = (UserT2)auth.GetUserProfile(context);
				} catch (EmailAlreadyUsedException) {
					OAuthUtils.DoRedirect(context, context.GetConfigValue("t2portal-emailAlreadyUsedEndpoint"), false);
				}

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
            return OAuthUtils.DoRedirect(context, redirect, false);
        }

        public object Get (OauthEverestDeleteRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            var redirect = "";
            try {
                context.Open ();

                context.LogInfo(this, string.Format("/everest/logout GET"));

                Connect2IdClient client = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
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
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return OAuthUtils.DoRedirect(context, redirect, false);
        }

    }
}

