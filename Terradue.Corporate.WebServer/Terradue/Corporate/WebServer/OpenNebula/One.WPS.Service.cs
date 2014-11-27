using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;
using System.Xml;
using Terradue.Corporate.Controller;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class OneWPSService : ServiceStack.ServiceInterface.Service {

        public object Get(GetWPS4One request) {
            List<WpsProcessOffering> result = new List<WpsProcessOffering>();

            IfyWebContext context = TepQWWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();

                WPSTep wpsFinder = new WPSTep(context);
                result = wpsFinder.GetWPSFromVMs();

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }



    }

    [Route("/one/wps", "GET", Summary = "GET a list of WPS services for the user", Notes = "Get list of OpenNebula WPS")]
    public class GetWPS4One : IReturn<List<WpsProcessOffering>> {}

    [Route("/one/user/{id}", "POST", Summary = "GET a user of opennebula", Notes = "Get OpenNebula user")]
    public class CreateWPSProcess : IReturn<List<string>> {
        [ApiMember(Name = "id", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }
}