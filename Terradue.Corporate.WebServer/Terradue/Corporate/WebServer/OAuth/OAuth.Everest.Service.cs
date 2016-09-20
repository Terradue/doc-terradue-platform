using System;
using ServiceStack.ServiceHost;
using Terradue.Ldap;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Terradue.Corporate.Controller;
using Terradue.Authentication.OAuth;
using ServiceStack.Text;

namespace Terradue.Corporate.WebServer {

    [Route("/everest/cb", "GET")]
    public class OautheverestCallBackRequest {
        [ApiMember(Name = "code", Description = "oauth code", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Code { get; set; }

        [ApiMember(Name = "state", Description = "oauth state", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string State { get; set; }

        [ApiMember(Name="ajax", Description = "ajax", ParameterType = "path", DataType = "bool", IsRequired = true)]
        public bool ajax { get; set; }

        [ApiMember(Name="error", Description = "error", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string error { get; set; }
    }

    [Route("/everest/sso", "GET")]
    public class OauthEverestSsoRequest {

        [ApiMember(Name = "return_to", Description = "return_to url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string return_to { get; set; }
    }

    [Route("/everest/logout", "GET")]
    public class OauthEverestDeleteRequest {

        [ApiMember(Name = "kind", Description = "logout kind url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string kind { get; set; }

        [ApiMember(Name = "message", Description = "logout kind url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string message { get; set; }
    }

    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthEverestService : ServiceStack.ServiceInterface.Service {
        public object Get(OautheverestSsoRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();
                var client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                HttpContext.Current.Session["everest-return_to"] = request.return_to;
                    
                //redirect to t2 portal SSO
                using (var service = base.ResolveService<OAuthGatewayService>()) { 
                    var response = service.Get(new OAuthAuthorizationRequest{
                        client_id = context.GetConfigValue
                            ("sso-clientId"),
                        response_type = "code",
                        nonce = Guid.NewGuid().ToString(),
                        state = Guid.NewGuid().ToString(),
                        redirect_uri = HttpUtility.UrlEncode(context.BaseUrl + "/t2api/everest/cb"),
                        ajax = false
                    });
                }; 

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return null;
        }

        public object Get(OauthEverestCallBackRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            var redirect = "";
            UserT2 user = null;
            try {
                context.Open();

                if(!string.IsNullOrEmpty(request.error)){
                    context.EndSession();
                    HttpContext.Current.Response.Redirect(context.BaseUrl, true);
                }

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");
                client.RedirectUri = context.BaseUrl + "/t2api/everest/cb";
                client.AccessToken(request.Code);

                OAuth2AuthenticationType auth = new OAuth2AuthenticationType(context);
                auth.SetConnect2IdCLient(client);

                user = (UserT2)auth.GetUserProfile(context);
                user.LoadLdapInfo();//TODO: should be done automatically on the previous call
                user.Store();

                var return_to = HttpContext.Current.Session["everest-return_to"];

                //build payload

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            HttpContext.Current.Response.Redirect(redirect, true);
            return null;
        }

        public object Get(OauthEverestDeleteRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            var redirect = "";
            try {
                context.Open();

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                if(!string.IsNullOrEmpty(request.kind) && request.kind.Equals("error")){
                    redirect = context.BaseUrl + "/portal/error?msg=Error%20from%20everest&longmsg=" + request.message;
                } else {
                    redirect = context.BaseUrl;
                }

                context.EndSession();
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            HttpContext.Current.Response.Redirect(redirect, true);
            return null;
        }

    }
}

