using System;
using ServiceStack.ServiceHost;
using Terradue.Corporate.Controller;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using Terradue.Ldap;
using ServiceStack.Common.Web;
using System.Web;
using System.Collections.Generic;
using Terradue.Authentication.OAuth;

namespace Terradue.Corporate.WebServer {


    /*
"www.terradue.com" -> www.terradue.com : Click on sign-in
"www.terradue.com" -> Webserver : GET /t2api/oauth
alt user not logged
Webserver -> "www.terradue.com/login" : redirect (+ query in params)
"www.terradue.com/login" -> Webserver : POST /t2api/oauth/login
Webserver -> LDAP : Authenticate
LDAP -> Webserver : OK
Webserver -> Callback : return code
else user must consent
Webserver -> "www.terradue.com/login/consent" : query + consents
"www.terradue.com/login/consent" -> Webserver : GET /t2api/oauth/consent
Webserver -> Callback : return code
else user logged
Webserver -> Callback : return code
end
    */

    [Route("/oauth/login", "POST", Summary = "login", Notes = "Login to the platform with username/password")]
    public class OauthLoginRequest : IReturn<Terradue.WebService.Model.WebUser>
    {
        [ApiMember(Name="username", Description = "username", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String username { get; set; }

        [ApiMember(Name="password", Description = "password", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String password { get; set; }

        [ApiMember(Name="query", Description = "Query string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String query { get; set; }
    }

    [Route("/oauth/consent", "POST", Summary = "login", Notes = "Login to the platform with username/password")]
    public class OauthConsentRequest : IReturn<Terradue.WebService.Model.WebUser>
    {
        [ApiMember(Name="scope", Description = "scope", ParameterType = "path", DataType = "String", IsRequired = true)]
        public List<String> scope { get; set; }//TODO: handle scope/claims (new/previsous/...)

        [ApiMember(Name="query", Description = "Query string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String query { get; set; }
    }

    [Route("/oauth/consent/current", "GET")]
    public class OauthCurrentConsentRequest {}

    [Route("/oauth", "GET", Summary = "login", Notes = "")]
    public class OAuthAuthorizationRequest : IReturn<Terradue.WebService.Model.WebUser>
    {
        [ApiMember(Name="client_id", Description = "Client id", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String client_id { get; set; }

        [ApiMember(Name="redirect_uri", Description = "Redirect uri", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String redirect_uri { get; set; }

        [ApiMember(Name="state", Description = "State", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String state { get; set; }

        [ApiMember(Name="scope", Description = "Scope", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String scope { get; set; }

        [ApiMember(Name="response_type", Description = "Response type", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String response_type { get; set; }
    }

    [Route("/oauth/config", "GET")]
    public class OauthConfigRequest {}

    [Route("/cb", "GET")]
    public class OauthCallBackRequest {
        [ApiMember(Name = "code", Description = "oauth code", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Code { get; set; }

        [ApiMember(Name = "state", Description = "oauth state", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string State { get; set; }
    }

    [Route("/auth", "DELETE", Summary = "logout", Notes = "Logout from the platform")]
    public class OauthLogoutRequest : IReturn<String> {
    }

    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthService : ServiceStack.ServiceInterface.Service {

        public object Post(OauthLoginRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");
                client.LdapAuthEndpoint = context.GetConfigValue("ldap-authEndpoint");
                client.LdapApiKey = context.GetConfigValue("ldap-apikey");

                var query = request.query;
                //TODO: temporary
                if(query == null) query = "?client_id=fcbwxxrenpgoy&redirect_uri=http%3A%2F%2F127.0.0.1%3A8081%2Ft2api%2Fcb&state=oKp0uEQVjrzPW5Q7f1c05w&scope=openid&response_type=code";

                OauthAuthzPostSessionRequest oauthrequest1 = new OauthAuthzPostSessionRequest {
                    query = query,
                    sub_sid = client.SESSIONSID
                };

                var response = client.AuthzSession(oauthrequest1);

                if(response.type == "auth"){
                    var user = client.Authenticate(request.username, request.password);

                    OauthAuthzPutSessionRequest oauthrequest2 = new OauthAuthzPutSessionRequest {
                        sub = user.Username,
                        acr = "1",
                        amr = new List<string>{ "ldap" },
                        data = new OauthUserInfoResponse {
                            name = user.Name,
                            email = user.Email
                        }
                    };

                    response = client.AuthzSession(response.sid, oauthrequest2);
                }

                if(response.type == "consent"){
                    //redirect to T2 consent page
//                    result.Headers[HttpHeaders.Location] = context.GetConfigValue("t2portal-consentEndpoint") + "?query=" + HttpUtility.UrlEncode(query);

                    //TODO: temporary
                    var consent = new Terradue.Ldap.OauthConsentRequest {
                        scope = new List<string>{"openid"},
                        claims = new List<string>(),
                        preset_claims = new OauthPresetClaims{
                            
                        },
                        audience = new List<string>(),
                        long_lived = true,
                        refresh_token = new OauthRefreshToken{
                            issue = true,
                            lifetime = 600
                        },
                        access_token = new OauthAccessToken{
                            encoding = "SELF_CONTAINED",
                            lifetime = 600
                        }
                    };

                    var redirect = client.ConsentSession(response.sid, consent);
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return null;
        }

        public object Post(OauthConsentRequest request){
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");
                client.LdapAuthEndpoint = context.GetConfigValue("ldap-authEndpoint");
                client.LdapApiKey = context.GetConfigValue("ldap-apikey");

                OauthAuthzPostSessionRequest oauthrequest = new OauthAuthzPostSessionRequest {
                    query = request.query,
                    sub_sid = client.SESSIONSID
                };

                var response = client.AuthzSession(oauthrequest);
                var consent = new Terradue.Ldap.OauthConsentRequest{
                    scope = request.scope
                };
                var redirect = client.ConsentSession(response.sid, consent);

                if(redirect != null){
                    //redirect to redirect_uri
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return null;
        }

        public object Get(OAuthAuthorizationRequest request){
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);

            try {
                context.Open();

                var client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");
                client.LdapAuthEndpoint = context.GetConfigValue("ldap-authEndpoint");
                client.LdapApiKey = context.GetConfigValue("ldap-apikey");

                var client_id = request.client_id ?? context.GetConfigValue("sso-clientId");
                var response_type = request.response_type ?? "code";
                var scope = request.scope ?? "openid";
                var state = request.state ?? Guid.NewGuid().ToString();
                var redirect_uri = request.redirect_uri ?? context.GetConfigValue("sso-callback");

                var query = string.Format("response_type={0}&scope={1}&client_id={2}&state={3}&redirect_uri={4}",
                                          response_type, scope, client_id, state, redirect_uri);

                var oauthrequest = new OauthAuthzPostSessionRequest {
                    query = query,
                    sub_sid = client.SESSIONSID
                };

                var oauthsession = client.AuthzSession(oauthrequest);

                //session is not active
                if (oauthsession.type == "auth"){
                    //redirect to T2 login page
                    var redirect = context.GetConfigValue("t2portal-loginEndpoint") + "?query=" + HttpUtility.UrlEncode(query);
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                else if (oauthsession.type == "consent"){
                    //redirect to T2 consent page
//                    result.Headers[HttpHeaders.Location] = context.GetConfigValue("t2portal-consentEndpoint") + "?query=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Url.Query);
                    var consent = new Terradue.Ldap.OauthConsentRequest{
                        scope = new List<string>{ "openid" }
                    };
                    var redirect = client.ConsentSession(oauthsession.sid, consent);
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                //session is still active
                else if (oauthsession.sub_session != null){
                    //redirect to redirect_uri
                    var redirect = request.redirect_uri;// + "?state=" + request.state + "&code=" + code;
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return null;
        }

        public object Get(OauthCallBackRequest request) {

            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            Terradue.Portal.User user = null;
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");
                client.LdapAuthEndpoint = context.GetConfigValue("ldap-authEndpoint");
                client.LdapApiKey = context.GetConfigValue("ldap-apikey");
                client.RedirectUri = context.GetConfigValue("sso-callback");
                client.AccessToken(request.Code);

                OAuth2AuthenticationType auth = new OAuth2AuthenticationType(context);
                auth.SetConnect2IdCLient(client);

                user = auth.GetUserProfile(context);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return user;
        }   

        public object Get(OauthConfigRequest request) {

            OauthConfigurationResponse response;

            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient();
                response = client.LoadConfiguration(context.GetConfigValue("sso-configUrl"));

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return response;
        }

        public object Delete(OauthLogoutRequest request) {
            T2CorporateWebContext wsContext = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                wsContext.Open();
                wsContext.EndSession();
                wsContext.Close();
            } catch (Exception e) {
                wsContext.Close();
                throw e;
            }
            return true;
        }

        private string BuildOauthQuery(IfyContext context){
            var query = "";
            query += "?client_id=" + context.GetConfigValue("sso-clientId");
            query += "&redirect_uri=" + HttpUtility.UrlEncode(context.GetConfigValue("sso-callback"));
            query += "&state=" + Guid.NewGuid();
            query += "&scope=openid";
            query += "&response_type=code";
            return query;
        }
    }
}

