using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using Terradue.Corporate.Controller;
using Terradue.WebService.Model;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class ConfigutarionService : ServiceStack.ServiceInterface.Service {
        
        public object Get(GetRoles request) {
            List<WebRole> result = new List<WebRole>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                PlanFactory pfactory = new PlanFactory(context);
                foreach(var role in pfactory.GetAllRoles()){
                    result.Add (new WebRole (role));
                }

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get (GetDomains request)
        {
            List<WebDomain> result = new List<WebDomain> ();

            IfyWebContext context = T2CorporateWebContext.GetWebContext (PagePrivileges.EverybodyView);
            try {
                context.Open ();

                PlanFactory pfactory = new PlanFactory (context);
                foreach (var domain in pfactory.GetAllDomains ()) {
                    result.Add (new WebDomain (domain));
                }

                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }



    }

    [Route("/plans", "GET", Summary = "GET the current roles", Notes = "")]
    public class GetRoles : IReturn<List<WebRole>> {}

    [Route ("/domains", "GET", Summary = "GET the current domains", Notes = "")]
    public class GetDomains : IReturn<List<WebDomain>> { }
}

