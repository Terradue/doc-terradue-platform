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
using Terradue.Crowd;
using System.Web;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue corporate portal")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserCrowdService : ServiceStack.ServiceInterface.Service {

        public object Get(GetCrowdUser request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);

            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, context.UserId);
                CrowdClient client = new CrowdClient(context.GetConfigValue("Crowd-api-url"), context.GetConfigValue("Crowd-app-name"), context.GetConfigValue("Crowd-app-pwd"));
                client.GetUser("eboissier");
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return true;
        }

//        public object Get(AuthenticateCrowdUser request){
//            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
//
//            try {
//                context.Open();
//                CrowdClient client = new CrowdClient("http://ldap.terradue.int:8095/crowd/rest", "enguecrowd", "enguercrowd");
//                string session = (string)client.Authenticate(request.username, request.password);
////                HttpContext.Current.Response.Cookies.Add(new HttpCookie(CrowdClient.COOKIE, session));
//                CrowdUser cUser = client.GetUser(request.username);
//                UserT2 user = UserT2.FromUsername(context, cUser.email);
//
//                context.Close();
//            } catch (Exception e) {
//                context.Close();
//                throw e;
//            }
//
//            return true;
//        }

    }
}

