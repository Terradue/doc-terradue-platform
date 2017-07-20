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

        [ApiMember(Name="nonce", Description = "Scope", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String nonce { get; set; }

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

    [Route ("/oauth/logout", "GET", Summary = "login", Notes = "")]
    [Route("/oauth", "DELETE", Summary = "login", Notes = "")]
    public class OAuthDeleteAuthorizationRequest
    {
        [ApiMember(Name="client_id", Description = "Client id", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String client_id { get; set; }

        [ApiMember(Name="redirect_uri", Description = "Redirect uri", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String redirect_uri { get; set; }

        [ApiMember(Name="state", Description = "State", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String state { get; set; }

        [ApiMember(Name="scope", Description = "Scope", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String scope { get; set; }

        [ApiMember(Name="nonce", Description = "Scope", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String nonce { get; set; }

        [ApiMember(Name="response_type", Description = "Response type", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String response_type { get; set; }

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

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// GET Oauth request. Get user session authorization 
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(OAuthAuthorizationRequest request){
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);

            try {
                context.Open();
                context.LogInfo (this, string.Format ("/oauth GET"));

                var client = new Connect2IdClient(context, context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                var client_id = request.client_id ?? context.GetConfigValue("sso-clientId");
                var response_type = request.response_type ?? "code";
                var nonce = request.nonce ?? Guid.NewGuid().ToString();
                var scope = request.scope ?? context.GetConfigValue("sso-scopes").Replace(","," ");
                var state = request.state ?? Guid.NewGuid().ToString();
                var redirect_uri = !string.IsNullOrEmpty(request.redirect_uri) ? HttpUtility.UrlEncode(request.redirect_uri) : HttpUtility.UrlEncode(context.GetConfigValue("sso-callback"));

                var query = string.Format("response_type={0}&scope={1}&client_id={2}&state={3}&redirect_uri={4}&nonce={5}",
                                          response_type, scope, client_id, state, redirect_uri, nonce);

                var oauthrequest = new OauthAuthzPostSessionRequest {
                    query = query
                };

                context.LogInfo(this, string.Format("/oauth (GET)"));
                context.LogDebug(this, string.Format("query = {0}",query));
                var subsid = client.LoadSUBSID ().Value;
                if(!string.IsNullOrEmpty(subsid)) oauthrequest.sub_sid = subsid; 

                var oauthsession = client.AuthzSession(oauthrequest, request.ajax);
                if(!string.IsNullOrEmpty(oauthsession.redirect)) return OAuthUtils.DoRedirect(context, oauthsession.redirect, request.ajax);
                if (!string.IsNullOrEmpty (oauthsession.error)) throw new Exception (oauthsession.error + " - " + oauthsession.error_description);
                var sid = oauthsession.sid;
                client.StoreSID (sid);
                context.LogDebug (this, string.Format ("SID = {0} ; SESSION = {1}", sid, HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : ""));
                if (oauthsession.error != null){
                    context.LogError(this, string.Format("{0}",oauthsession.error));
                    context.LogError(this, string.Format("{0}",oauthsession.error_description));
                }

                //session is not active
                if (oauthsession.error != null || oauthsession.type == "auth"){
                    //redirect to T2 login page
                    var redirect = context.GetConfigValue("t2portal-loginEndpoint") + "?query=" + HttpUtility.UrlEncode(query) + "&type=auth";
                    context.LogDebug(this, string.Format("type = auth"));
                    return OAuthUtils.DoRedirect(context, redirect, request.ajax);
                }

                else if (oauthsession.type == "consent"){
                    if (oauthsession.sub_session != null) {
                        subsid = oauthsession.sub_session.sid;
                        client.StoreSUBSID (subsid);
                    }
                    context.LogDebug(this, string.Format("type = consent"));
                    context.LogDebug(this, string.Format("{0} new claims to consent : {1}", oauthsession.scope.new_claims.Count, string.Join(",",oauthsession.scope.new_claims)));

                    //no new scope to consent
                    if(oauthsession.scope.new_claims.Count == 0 
                       || (oauthsession.scope.new_claims.Count == 1 && oauthsession.scope.new_claims[0].Equals("openid"))){
                        var scopes = new List<string>(context.GetConfigValue("sso-scopes").Split(",".ToCharArray()));
                        var consent = GenerateConsent(scopes);
                        var redirect = client.ConsentSession(oauthsession.sid, consent);
                        return OAuthUtils.DoRedirect(context, redirect, request.ajax);
                    } else {
                        //redirect to T2 consent page
                        var redirect = context.GetConfigValue("t2portal-loginEndpoint") + "?query=" + HttpUtility.UrlEncode(query) + "&type=consent";
                        return OAuthUtils.DoRedirect(context, redirect, request.ajax);
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
                context.LogError(this, e.Message + " - " + e.StackTrace);
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
                context.LogInfo (this, string.Format ("/oauth POST Username='{0}'", request.username));

                Connect2IdClient client = new Connect2IdClient(context, context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                var defaultscopes = new List<string>(context.GetConfigValue("sso-scopes").Split(",".ToCharArray()));
                var query = HttpUtility.UrlDecode(request.query);

                var sid = client.LoadSID ().Value;
                var subsid = client.LoadSUBSID ().Value;


                context.LogInfo(this, string.Format("/oauth (POST) - username={0}", request.username));

                //request was done just to get the oauthsession (and the list of scopes to consent)
                if(request.username == null && request.password == null && request.scope == null){

                    context.LogDebug(this, string.Format("request was done just to get the oauthsession (and the list of scopes to consent)"));

                    OauthAuthzPostSessionRequest oauthrequest1 = new OauthAuthzPostSessionRequest {
                        query = query
                    };
                    if(!string.IsNullOrEmpty(subsid)) oauthrequest1.sub_sid = subsid;
                    return new HttpResult(client.AuthzSession(oauthrequest1), System.Net.HttpStatusCode.OK);
                }

                if(request.scope != null){
                    OauthConsentRequest consent = GenerateConsent(request.scope);

                    var redirect = client.ConsentSession(sid, consent);
                    return OAuthUtils.DoRedirect(context, redirect, request.ajax);
                }

                //user needs to authenticate
                LdapUser user = null;
                try{
                    var j2ldapclient = new LdapAuthClient(context.GetConfigValue("ldap-authEndpoint"));
                    user = j2ldapclient.Authenticate(request.username, request.password, context.GetConfigValue("ldap-apikey"));
                    context.LogDebug(this, string.Format("User {0} is authenticated succesfully", request.username));

                    //if user exists, sync Artifactory
                    try{
                        var usert2 = Terradue.Corporate.Controller.UserT2.FromUsernameOrEmail(context, request.username);
                        if (usert2 != null) usert2.SyncArtifactory(request.username, request.password);
                    }catch(Exception e){
                        context.LogError (this, string.Format ("{0} - {1}", e.Message, e.StackTrace));    
                    }

                }catch(Exception e){
                    context.LogError(this, string.Format("User {0} is not authenticated: {1}", request.username, e.Message));
                    return new HttpError(System.Net.HttpStatusCode.Forbidden, "Wrong username or password");
                }

                //we need to create the sub_SID
                OauthAuthzPutSessionRequest oauthrequest2 = new OauthAuthzPutSessionRequest {
                    sub = user.Username//,
//                    acr = "1",
//                    amr = new List<string>{ "ldap" },
//                    data = new OauthUserInfoResponse {
//                        name = user.Name,
//                        email = user.Email
//                    }
                };
                context.LogDebug(this, string.Format ("SID = {0} ; SESSION = {1}", sid, HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : ""));
                var oauthputsession = client.AuthzSession(sid, oauthrequest2, request.ajax);
                context.LogDebug(this, string.Format("SUBSID : {0}", subsid));
                if (!string.IsNullOrEmpty (oauthputsession.redirect)) {
                    return OAuthUtils.DoRedirect (context, oauthputsession.redirect, request.ajax);
                }
                //                    client.SID = null;
                if (oauthputsession.sub_session != null) {
                    subsid = oauthputsession.sub_session.sid;
                    client.StoreSUBSID (subsid);
                }

                //user is now authenticated and need to consent
                if(oauthputsession.type == "consent"){
                    context.LogDebug(this, string.Format("type = consent"));
                    context.LogDebug(this, string.Format("{0} new claims to consent : {1}", oauthputsession.scope.new_claims.Count, string.Join(",",oauthputsession.scope.new_claims)));
                    OauthConsentRequest consent = null;
                    if(oauthputsession.scope.new_claims.Count == 0){
                        consent = GenerateConsent(defaultscopes);
                    } else if(request.autoconsent) consent = GenerateConsent(oauthputsession.scope.new_claims);
                    else return new HttpResult(oauthputsession, System.Net.HttpStatusCode.OK);

                    context.LogDebug(this, string.Format("consent is now : {0}", string.Join(",",consent.scope)));
                        
                    var redirect = client.ConsentSession(oauthputsession.sid, consent);
                    return OAuthUtils.DoRedirect(context, redirect, request.ajax);
                    
                }

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
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
                context.LogInfo (this, string.Format ("/oauth/JWT POST"));
                Connect2IdClient client = new Connect2IdClient(context, context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                //we do the JWT token (this authenticates the client)
                client.JWTBearerToken(request.assertion);

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return token;
        }


        public object Delete(OAuthDeleteAuthorizationRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            string redirect = null;
            try {
                context.Open();
                context.LogInfo (this, string.Format ("/oauth DELETE"));

                context.EndSession();
                redirect = HttpContext.Current.Response.Headers [HttpHeaders.Location];

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return OAuthUtils.DoRedirect(context, redirect ?? context.BaseUrl, request.ajax);
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
                claims = new List<string>{ "openid", "email", "sshPublicKey", "given_name", "rn" },
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

