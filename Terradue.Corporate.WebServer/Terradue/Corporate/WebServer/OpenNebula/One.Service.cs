using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class OneService : ServiceStack.ServiceInterface.Service {

        public object Get(GetOneConfig request) {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();

                string key = "One-access";
                result.Add(new KeyValuePair<string, string>(key,context.GetConfigValue(key)));

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(TestAuth request) {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                OneClient one = new OneClient(context.GetConfigValue("One-xmlrpc-url"),request.User,request.Password);

                one.StartDelegate(request.Target);
                USER_POOL pool = one.UserGetPoolInfo();
                one.EndDelegate();

                foreach(object u in pool.Items){
                    if(u is USER_POOLUSER) result.Add(new KeyValuePair<string, string>((((USER_POOLUSER)u).ID),(((USER_POOLUSER)u).NAME)));
                }

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }
    }

    [Route("/one/config", "GET", Summary = "GET opennebula config", Notes = "Get OpenNebula config")]
    public class GetOneConfig : IReturn<List<KeyValuePair<string, string>>> {}

    [Route("/one/auth", "GET", Summary = "GET opennebula config", Notes = "Get OpenNebula config")]
    public class TestAuth : IReturn<WebResponseBool> {
        [ApiMember(Name = "usr", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public string User { get; set; }
        [ApiMember(Name = "target", Description = "User target", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Target { get; set; }
        [ApiMember(Name = "pwd", Description = "User password", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Password { get; set; }
    }

}