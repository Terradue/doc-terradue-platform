﻿using System;
using ServiceStack.ServiceHost;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using Terradue.Ldap;
using System.Web;
using System.Collections.Generic;
using ServiceStack.Common.Web;

namespace Terradue.Corporate.WebServer {

    [Route("/oauth", "GET", Summary = "login", Notes = "")]
    public class OAuthAuthorizationRequest
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
    public class OauthLoginRequest
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

        [ApiMember(Name="autoconsent", Description = "Automatically consent", ParameterType = "path", DataType = "bool", IsRequired = true)]
        public bool autoconsent { get; set; }
    }

    [Route("/oauth", "DELETE", Summary = "login", Notes = "")]
    public class OAuthDeleteAuthorizationRequest
    {
        [ApiMember(Name="query", Description = "Query string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String query { get; set; }

        [ApiMember(Name="ajax", Description = "ajax", ParameterType = "path", DataType = "bool", IsRequired = true)]
        public bool ajax { get; set; }
    }

    [Route("/oauth/JWT", "POST", Summary = "login", Notes = "")]
    public class OAuthJWTTokenRequest
    {
        [ApiMember(Name="grant_type", Description = "grant type string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String grant_type { get; set; }

        [ApiMember(Name="assertion", Description = "assertion string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String assertion { get; set; }

        [ApiMember(Name="scope", Description = "Scope", ParameterType = "path", DataType = "List<String>", IsRequired = true)]
        public List<String> scope { get; set; }

        [ApiMember(Name="eosso", Description = "eosso string", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String eossotype { get; set; }

        [ApiMember(Name="redirect_uri", Description = "Redirect uri", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String redirect_uri { get; set; }

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

                var client_id = request.client_id ?? context.GetConfigValue("sso-clientId");
                var response_type = request.response_type ?? "code";
                var scope = request.scope ?? context.GetConfigValue("sso-scopes").Replace(","," ");
//                scope = "openid email profile sshPublicKey rn";
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
                    if(oauthsession.scope.new_claims.Count == 0 
                       || (oauthsession.scope.new_claims.Count == 1 && oauthsession.scope.new_claims[0].Equals("openid"))){
                        var scopes = new List<string>(context.GetConfigValue("sso-scopes").Split(",".ToCharArray()));
                        var consent = GenerateConsent(scopes);
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

                var defaultscopes = new List<string>(context.GetConfigValue("sso-scopes").Split(",".ToCharArray()));

                var query = HttpUtility.UrlDecode(request.query);

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
                        var j2ldapclient = new LdapAuthClient(context.GetConfigValue("ldap-authEndpoint"));
                        user = j2ldapclient.Authenticate(request.username, request.password, context.GetConfigValue("ldap-apikey"));
                    }catch(Exception e){
                        return new HttpError(System.Net.HttpStatusCode.Forbidden, e);
                    }

//                    //JWT token
//                    string jwt = client.GetJWT(user.Username);
//                    client.JWTBearerToken(jwt);

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
                        OauthConsentRequest consent = null;
                        if(oauthsession.scope.new_claims.Count == 0){
                            consent = GenerateConsent(defaultscopes);
                        } else if(request.autoconsent) consent = GenerateConsent(oauthsession.scope.new_claims);
                        else return new HttpResult(oauthsession, System.Net.HttpStatusCode.OK);
                            
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
                }

                //user needs to consent
                if(oauthsession.type == "consent"){

                    var consent = new OauthConsentRequest();

                    if(request.scope != null)
                        consent = GenerateConsent(request.scope);
                    else if(oauthsession.scope.new_claims.Count == 0){
                        consent = GenerateConsent(defaultscopes);
                    } else if(request.autoconsent || (oauthsession.scope.new_claims.Count == 1 && oauthsession.scope.new_claims[0].Equals("openid")))
                        consent = GenerateConsent(oauthsession.scope.new_claims);
                    else if (request.username != null && request.password != null){
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

        public object Post(OAuthJWTTokenRequest request){
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            string token = null;
            try {
                context.Open();
                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                //we do the JWT token (this authenticates the client)
                client.JWTBearerToken(request.assertion);
                token = client.OAUTHTOKEN.access_token;

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return token;
        }


        public object Delete(OAuthDeleteAuthorizationRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                var query = HttpUtility.UrlDecode(request.query);

//                if(string.IsNullOrEmpty(query))
//                    query = string.Format("response_type={0}&scope={1}&client_id={2}&state={3}&redirect_uri={4}",
//                                          "code", context.GetConfigValue("sso-scopes"), context.GetConfigValue("sso-clientId"), Guid.NewGuid(), context.GetConfigValue("sso-callback"));

                OauthAuthzPostSessionRequest oauthrequest1 = new OauthAuthzPostSessionRequest {
                    query = query,
                    sub_sid = client.SESSIONSID
                };

                var oauthsession = client.AuthzSession(oauthrequest1);

                var redirect = client.DeleteAuthz(oauthsession.sid);

                if(request.ajax){
                    HttpResult redirectResponse = new HttpResult();
                    redirectResponse.Headers[HttpHeaders.Location] = redirect;
                    redirectResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                    return redirectResponse;
                } else {
                    HttpContext.Current.Response.Redirect(redirect, true);
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return null;
        }

        private OauthConsentRequest GenerateConsent(List<string> scope){

            bool openid = false;
            foreach (var s in scope)
                if (scope.Equals("openid"))
                    openid = true;
            if(!openid) scope.Add("openid");

            var consent = new OauthConsentRequest {
                scope = scope,
//                scope = new List<string>{ "openid", "email", "profile", "rn" },
//                claims = new List<string>{ "openid", "email", "sshPublicKey", "given_name", "rn" },
//                preset_claims = new OauthPresetClaims{
//                    userinfo = new OauthAuthzClaimsUserInfoRequest{
//                        sshPublicKey = ""
//                    }
//                },
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

