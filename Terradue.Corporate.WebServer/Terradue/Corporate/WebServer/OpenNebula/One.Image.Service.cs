using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class OneImageService : ServiceStack.ServiceInterface.Service {

        public object Get(GetImage4One request) {
            string result = null;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                context.LogInfo(this, string.Format("/one/img/{{id}} GET - id="+request.Id));
                OneClient one = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                IMAGE oneuser = one.ImageGetInfo(request.Id);
                result = oneuser.NAME;
                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }

    }

    [Route("/one/img/{id}", "GET", Summary = "GET a user of opennebula", Notes = "Get OpenNebula user")]
    public class GetImage4One : IReturn<List<string>> {
        [ApiMember(Name = "id", Description = "Image id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

}

