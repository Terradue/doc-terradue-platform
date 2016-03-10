using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;
using Terradue.Corporate.Controller;
using Terradue.Security.Certification;
using System.Net;
using System.IO;
using ServiceStack.Text;
using System.Runtime.Serialization;
using ServiceStack.Common.Web;
using Terradue.Ldap;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserSafeService : ServiceStack.ServiceInterface.Service {
       
        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(CreateSafeUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebSafe result;
            try{
                context.Open();

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                UserT2 user = UserT2.FromId(context, context.UserId);

                //authenticate user
                try{
                    var j2ldapclient = new LdapAuthClient(context.GetConfigValue("ldap-authEndpoint"));
                    var usr = j2ldapclient.Authenticate(user.Identifier, request.password, context.GetConfigValue("ldap-apikey"));
                }catch(Exception e){
                    throw new Exception("Invalid password");
                }

                user.CreateSafe();
                user.UpdateLdapAccount();    

                result = new WebSafe();
                result.PublicKey = user.PublicKey;
                result.PrivateKey = user.PrivateKey;

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

        public object Delete(DeleteSafeUserT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);

            try{
                context.Open();
                UserT2 user = UserT2.FromId(context, context.UserId);
                //authenticate user
                try{
                    var j2ldapclient = new LdapAuthClient(context.GetConfigValue("ldap-authEndpoint"));
                    var usr = j2ldapclient.Authenticate(user.Identifier, request.password, context.GetConfigValue("ldap-apikey"));
                }catch(Exception e){
                    throw new Exception("Invalid password");
                }
                user.DeletePublicKey(request.password);
                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public object Get(GetSafeUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            WebSafe result;
            try{
                context.Open();

                if(string.IsNullOrEmpty(request.token) || !request.token.Equals(context.GetConfigValue("t2-safe-token"))){
                    throw new Exception("Invalid token");
                }
                if(string.IsNullOrEmpty(request.username)){
                    throw new Exception("Invalid username");
                }

                Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");

                UserT2 user = null;
                try{
                    user = UserT2.FromUsername(context, request.username);
                }catch(Exception e){
                    throw new Exception("Invalid username");
                }

                user.LoadLdapInfo();

                result = new WebSafe();
                result.PublicKey = user.PublicKey;

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

    }
  
}

