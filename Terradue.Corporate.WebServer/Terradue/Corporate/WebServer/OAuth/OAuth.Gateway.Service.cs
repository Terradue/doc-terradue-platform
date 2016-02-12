using System;
using ServiceStack.ServiceHost;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using Terradue.Ldap;
using System.Web;
using System.Collections.Generic;
using ServiceStack.Common.Web;

namespace Terradue.Corporate.WebServer {

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

        [ApiMember(Name="ajax", Description = "ajax", ParameterType = "path", DataType = "bool", IsRequired = true)]
        public bool ajax { get; set; }
    }

    [Route("/oauth", "POST", Summary = "login", Notes = "Login to the platform with username/password")]
    public class OauthLoginRequest : IReturn<Terradue.WebService.Model.WebUser>
    {
        [ApiMember(Name="username", Description = "username", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String username { get; set; }

        [ApiMember(Name="password", Description = "password", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String password { get; set; }

        [ApiMember(Name="query", Description = "Query string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String query { get; set; }

        [ApiMember(Name="scope", Description = "Scope", ParameterType = "path", DataType = "List<String>", IsRequired = true)]
        public List<String> scope { get; set; }

        [ApiMember(Name="ajax", Description = "ajax", ParameterType = "path", DataType = "bool", IsRequired = true)]
        public bool ajax { get; set; }
    }

    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthGatewayService : ServiceStack.ServiceInterface.Service {

        /// <summary>
        /// GET Oauth request. Get user session authorization 
        /// </summary>
        /// <param name="request">Request.</param>
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

                var oauthsession = client.AuthzSession(oauthrequest, request.ajax);

                //session is not active
                if (oauthsession.type == "auth"){
                    //redirect to T2 login page
                    var redirect = context.GetConfigValue("t2portal-loginEndpoint") + "?query=" + HttpUtility.UrlEncode(query) + "&type=auth";
                    if(request.ajax){
                        HttpResult redirectResponse = new HttpResult();
                        redirectResponse.Headers[HttpHeaders.Location] = redirect;
                        redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                        return redirectResponse;
                    } else {
                        HttpContext.Current.Response.Redirect(redirect, true);
                    }
                }

                else if (oauthsession.type == "consent"){
                    //no new scope to consent
                    if(oauthsession.scope.new_claims.Count == 0){
                        var consent = GenerateConsent(null);
                        var redirect = client.ConsentSession(oauthsession.sid, consent);
                        if(request.ajax){
                            HttpResult redirectResponse = new HttpResult();
                            redirectResponse.Headers[HttpHeaders.Location] = redirect;
                            redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                            return redirectResponse;
                        } else {
                            HttpContext.Current.Response.Redirect(redirect, true);
                        }
                    } else {
                        //redirect to T2 consent page
                        var redirect = context.GetConfigValue("t2portal-loginEndpoint") + "?query=" + HttpUtility.UrlEncode(query) + "&type=consent";
                        if(request.ajax){
                            HttpResult redirectResponse = new HttpResult();
                            redirectResponse.Headers[HttpHeaders.Location] = redirect;
                            redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                            return redirectResponse;
                        } else {
                            HttpContext.Current.Response.Redirect(redirect, true);
                        }
                    }
                }

                //session is still active
                else if (oauthsession.sub_session != null){
                    //redirect to redirect_uri
                    var redirect = request.redirect_uri;
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return null;
        }

        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
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

                var query = HttpUtility.UrlDecode(request.query);
                //TODO: temporary
                if(query == null) query = "?client_id=fcbwxxrenpgoy&redirect_uri=http%3A%2F%2F127.0.0.1%3A8081%2Ft2api%2Fcb&state=oKp0uEQVjrzPW5Q7f1c05w&scope=openid&response_type=code";

                OauthAuthzPostSessionRequest oauthrequest1 = new OauthAuthzPostSessionRequest {
                    query = query,
                    sub_sid = client.SESSIONSID
                };

                var oauthsession = client.AuthzSession(oauthrequest1);

                //request was done just to get the oauthsession (and the list of scopes to consent)
                if(request.username == null && request.password == null && request.scope == null){
                    return new HttpResult(oauthsession, System.Net.HttpStatusCode.OK);
                }

                //user needs to authenticate
                if(oauthsession.type == "auth"){
                    LdapUser user = null;
                    try{
                        user = client.Authenticate(request.username, request.password);
                    }catch(Exception e){
                        return new HttpError(System.Net.HttpStatusCode.Forbidden, e);
                    }

                    OauthAuthzPutSessionRequest oauthrequest2 = new OauthAuthzPutSessionRequest {
                        sub = user.Username,
                        acr = "1",
                        amr = new List<string>{ "ldap" },
                        data = new OauthUserInfoResponse {
                            name = user.Name,
                            email = user.Email
                        }
                    };

                    oauthsession = client.AuthzSession(oauthsession.sid, oauthrequest2);

                    //user is now authenticated and need to consent
                    if(oauthsession.type == "consent"){
                        if(oauthsession.scope.new_claims.Count == 0){
                            var consent = GenerateConsent(null);
                            var redirect = client.ConsentSession(oauthsession.sid, consent);

                            if(request.ajax){
                                HttpResult redirectResponse = new HttpResult();
                                redirectResponse.Headers[HttpHeaders.Location] = redirect;
                                redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                                return redirectResponse;
                            } else {
                                HttpContext.Current.Response.Redirect(redirect, true);
                            }
                        } else {
                            return new HttpResult(oauthsession, System.Net.HttpStatusCode.OK);
                        }
                    }
                }

                //user needs to consent
                if(oauthsession.type == "consent"){

                    var consent = new OauthConsentRequest();

                    if(request.scope != null){
                        consent = GenerateConsent(request.scope);
                    } else if(oauthsession.scope.new_claims.Count == 0){
                        consent = GenerateConsent(null);
                    } else if (request.username != null && request.password != null){
                        //return to the login page so it can display the consent
                        return new HttpResult(oauthsession, System.Net.HttpStatusCode.OK);
                    } else {
                        throw new Exception("Not expected behaviour");
                    }

                    var redirect = client.ConsentSession(oauthsession.sid, consent);

                    if(request.ajax){
                        HttpResult redirectResponse = new HttpResult();
                        redirectResponse.Headers[HttpHeaders.Location] = redirect;
                        redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                        return redirectResponse;
                    } else {
                        HttpContext.Current.Response.Redirect(redirect, true);
                    }
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return null;
        }

        private OauthConsentRequest GenerateConsent(List<string> scope){
            if (scope == null) scope = new List<string>();

            var consent = new OauthConsentRequest {
                scope = scope,
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
            return consent;
        }

    }
}

