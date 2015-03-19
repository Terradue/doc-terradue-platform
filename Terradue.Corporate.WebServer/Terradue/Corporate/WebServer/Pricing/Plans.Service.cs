using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.Corporate.WebServer {
    [Api("Tep-QuickWin Terradue webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class ConfigutarionService : ServiceStack.ServiceInterface.Service {
        
        public object Get(GetPlans request) {
            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                result.Add(new KeyValuePair<string, int>("Developer", (int)PlanType.DEVELOPER));
                result.Add(new KeyValuePair<string, int>("Integrator", (int)PlanType.INTEGRATOR));
                result.Add(new KeyValuePair<string, int>("Provider", (int)PlanType.PROVIDER));

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

