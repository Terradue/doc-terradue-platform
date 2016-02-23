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
                client.LdapAuthEndpoint = context.GetConfigValue("ldap-authEndpoint");
                client.LdapApiKey = context.GetConfigValue("ldap-apikey");

                UserT2 user = UserT2.FromId(context, context.UserId);

                //authenticate user
                try{
                    client.Authenticate(user.Identifier, request.password);
                }catch(Exception e){
                    throw new Exception("Invalid password");
                }

                user.CreateSafe();
                result = new WebSafe();
                result.PublicKey = user.GetPublicKey();
                result.PrivateKey = user.GetPrivateKey();

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

    }
  
}

