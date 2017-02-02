using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.TepQW.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class ConfigutarionService : ServiceStack.ServiceInterface.Service {

        public object Get(GetConfig request) {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                result.Add(new KeyValuePair<string, string>("Github-client-id",context.GetConfigValue("Github-client-id")));
                result.Add(new KeyValuePair<string, string>("reCaptcha-public",context.GetConfigValue("reCaptcha-public")));

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }

    }
}