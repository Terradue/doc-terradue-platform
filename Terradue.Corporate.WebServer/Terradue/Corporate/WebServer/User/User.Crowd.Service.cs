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
using Terradue.Crowd.WebService;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue corporate portal")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserCrowdService : ServiceStack.ServiceInterface.Service {

        public object Get(Terradue.Crowd.WebService.GetCrowdUser request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);

            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, context.UserId);
                CrowdClient client = new CrowdClient(context.GetConfigValue("Crowd-api-url"), context.GetConfigValue("Crowd-app-name"), context.GetConfigValue("Crowd-app-pwd"));
                client.GetUser(user.Identifier);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return true;
        }

        public object Post(CrowdCreateUser request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);

            try {
                context.Open();
                CrowdClient client = new CrowdClient(context.GetConfigValue("Crowd-api-url"), context.GetConfigValue("Crowd-app-name"), context.GetConfigValue("Crowd-app-pwd"));
                client.CreateUser(request);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return true;
        }

        public object Post(CrowdCreateUserFromDb request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);

            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, request.id);
                CrowdUser cuser = new CrowdUser();
                cuser.active = true;
                cuser.email = user.Email;
                cuser.display_name = user.Username;
                cuser.first_name = user.FirstName;
                cuser.last_name = user.LastName;
                cuser.name = user.Username;
                cuser.password = "changeme"; //TODO: get real password

                CrowdClient client = new CrowdClient(context.GetConfigValue("Crowd-api-url"), context.GetConfigValue("Crowd-app-name"), context.GetConfigValue("Crowd-app-pwd"));
                client.CreateUser();
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return true;
        }

    }
}

