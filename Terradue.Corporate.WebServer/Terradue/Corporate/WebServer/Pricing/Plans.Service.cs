using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using Terradue.Corporate.Controller;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class ConfigutarionService : ServiceStack.ServiceInterface.Service {
        
        public object Get(GetPlans request) {
            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                PlanFactory pfactory = new PlanFactory(context);
                foreach(var plan in pfactory.GetAllPlans()){
                    result.Add(new KeyValuePair<string, int>(plan.Name, plan.Id));
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }
    }

    [Route("/plans", "GET", Summary = "GET the current plans", Notes = "")]
    public class GetPlans : IReturn<List<KeyValuePair<string, int>>> {}
}

