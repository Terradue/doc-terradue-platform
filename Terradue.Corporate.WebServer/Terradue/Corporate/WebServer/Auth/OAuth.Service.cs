using System;
using ServiceStack.ServiceHost;
using Terradue.Corporate.Controller;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using Terradue.Ldap;
using ServiceStack.Common.Web;
using System.Web;
using System.Collections.Generic;

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

    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthService : ServiceStack.ServiceInterface.Service {

        public object Post(T2LoginRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            var result = new HttpResult();
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient();

                OauthAuthzPostSessionRequest oauthrequest1 = new OauthAuthzPostSessionRequest {
                    query = request.query,
                    sub_sid = client.SESSION_SID
                };

                var response1 = client.AuthzSession(oauthrequest1);
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

                if(response2.type == "consent"){
                    //redirect to T2 consent page
                    result.Headers[HttpHeaders.Location] = context.GetConfigValue("t2portal-consentEndpoint") + "?query=" + HttpUtility.UrlEncode(request.query);
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Post(T2ConsentRequest request){
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            var result = new HttpResult();
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient();

                OauthAuthzPostSessionRequest oauthrequest = new OauthAuthzPostSessionRequest {
                    query = request.query,
                    sub_sid = client.SESSION_SID
                };

                var response1 = client.AuthzSession(oauthrequest);
                var consent = new OauthConsentRequest{
                    scope = request.scope
                };
                var response = client.ConsentSession(response1.sid, consent);

                if(response != null){
                    //redirect to redirect_uri
                    result.Headers[HttpHeaders.Location] = response;
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(T2OAuthLoginRequest request){
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            var result = new HttpResult();

            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient();

                OauthAuthzPostSessionRequest oauthrequest = new OauthAuthzPostSessionRequest {
                    query = HttpContext.Current.Request.Url.Query,
                    sub_sid = client.SESSION_SID
                };

                var oauthsession = client.AuthzSession(oauthrequest);

                //session is not active
                if (oauthsession.type == "auth"){
                    //redirect to T2 login page
                    result.Headers[HttpHeaders.Location] = context.GetConfigValue("t2portal-loginEndpoint") + "?query_string=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Url.Query);
                }

                else if (oauthsession.type == "consent"){
                    //redirect to T2 consent page
//                    result.Headers[HttpHeaders.Location] = context.GetConfigValue("t2portal-consentEndpoint") + "?query_string=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Url.Query);
                    var consent = new OauthConsentRequest{
                        scope = new List<string>{ "openid" }
                    };
                    var consentResponse = client.ConsentSession(oauthsession.sid, consent);
                    result.Headers[HttpHeaders.Location] = consentResponse;
                }

                //session is still active
                else if (oauthsession.sub_session != null){
                    //redirect to redirect_uri
                    result.Headers[HttpHeaders.Location] = request.redirect_uri;// + "?state=" + request.state + "&code=" + code;
                }
                    
                else {
                    //Should not come here
                    result = null;
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return result;

        }
    }
}

