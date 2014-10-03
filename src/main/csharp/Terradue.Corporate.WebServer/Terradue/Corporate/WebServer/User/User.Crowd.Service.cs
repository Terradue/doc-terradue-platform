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

    }
}

