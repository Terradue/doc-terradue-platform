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

    [Route("/oauth/login", "POST", Summary = "login", Notes = "Login to the platform with username/password")]
    public class T2LoginRequest : IReturn<Terradue.WebService.Model.WebUser>
    {
        [ApiMember(Name="username", Description = "username", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String username { get; set; }

        [ApiMember(Name="password", Description = "password", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String password { get; set; }

        [ApiMember(Name="query", Description = "Query string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String query { get; set; }
    }

    [Route("/oauth/consent", "POST", Summary = "login", Notes = "Login to the platform with username/password")]
    public class T2ConsentRequest : IReturn<Terradue.WebService.Model.WebUser>
    {
        [ApiMember(Name="scope", Description = "scope", ParameterType = "path", DataType = "String", IsRequired = true)]
        public List<String> scope { get; set; }//TODO: handle scope/claims (new/previsous/...)

        [ApiMember(Name="query", Description = "Query string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String query { get; set; }
    }

    [Route("/oauth", "GET", Summary = "login", Notes = "")]
    public class T2OAuthLoginRequest : IReturn<Terradue.WebService.Model.WebUser>
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
    public class CallBack {
        [ApiMember(Name = "code", Description = "oauth code", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Code { get; set; }

        [ApiMember(Name = "state", Description = "oauth state", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string State { get; set; }

    }

    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthService : ServiceStack.ServiceInterface.Service {

        public object Post(T2LoginRequest request) {
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

                var response1 = client.AuthzSession(oauthrequest1);

                var sid = response1.sid;
                var type = response1.type;

                if(type == "auth"){
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

                    var response2 = client.AuthzSession(response1.sid, oauthrequest2);

                    type = response2.type;
                    sid = response2.sid;
                }

                if(type == "consent"){
                    //redirect to T2 consent page
//                    result.Headers[HttpHeaders.Location] = context.GetConfigValue("t2portal-consentEndpoint") + "?query=" + HttpUtility.UrlEncode(query);

                    //TODO: temporary
                    OauthConsentRequest consent = new OauthConsentRequest {
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

                    var redirect = client.ConsentSession(sid, consent);
                    //we will never arrive there as the consent does the redirect
//                    result.Headers[HttpHeaders.Location] = redirect;
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return null;
        }

        public object Post(T2ConsentRequest request){
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

                var response1 = client.AuthzSession(oauthrequest);
                var consent = new OauthConsentRequest{
                    scope = request.scope
                };
                var redirect = client.ConsentSession(response1.sid, consent);

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

        public object Get(T2OAuthLoginRequest request){
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
                    query = HttpContext.Current.Request.Url.Query,
                    sub_sid = client.SESSIONSID
                };

                var oauthsession = client.AuthzSession(oauthrequest);

                //session is not active
                if (oauthsession.type == "auth"){
                    //redirect to T2 login page
                    var redirect = context.GetConfigValue("t2portal-loginEndpoint") + "?query_string=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Url.Query);
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                else if (oauthsession.type == "consent"){
                    //redirect to T2 consent page
//                    result.Headers[HttpHeaders.Location] = context.GetConfigValue("t2portal-consentEndpoint") + "?query_string=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Url.Query);
                    var consent = new OauthConsentRequest{
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

        public object Get(CallBack request) {

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
    }
}

