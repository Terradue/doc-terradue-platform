using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;

namespace Terradue.Corporate.WebServer {
    [Api("Tep-QuickWin Terradue webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class OneUserService : ServiceStack.ServiceInterface.Service {

        public object Get(GetUsers4One request) {
            List<System.Collections.Generic.KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                OneClient one = new OneClient(context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                USER_POOL pool = one.UserGetPoolInfo();
                foreach(object u in pool.Items){
                    if(u is USER_POOLUSER) result.Add(new KeyValuePair<string, string>((((USER_POOLUSER)u).ID),(((USER_POOLUSER)u).NAME)));
                }
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(GetUser4One request) {
            string result = null;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                OneClient one = new OneClient(context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                USER oneuser = one.UserGetInfo(request.Id);
                result = oneuser.NAME;
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Put(UpdateUser4OnePassword request) {
            bool result;
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                OneClient one = new OneClient(context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                result = one.UserUpdatePassword(request.Id, request.Password);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

    }

    [Route("/one/user", "GET", Summary = "GET a list of users of opennebula", Notes = "Get list of OpenNebula users")]
    public class GetUsers4One : IReturn<List<System.Collections.Generic.KeyValuePair<string, string>>> {}

    [Route("/one/user/{id}", "GET", Summary = "GET a user of opennebula", Notes = "Get OpenNebula user")]
    public class GetUser4One : IReturn<List<string>> {
        [ApiMember(Name = "id", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    [Route("/one/user/pwd", "PUT", Summary = "PUT the password of an opennebula user", Notes = "")]
    public class UpdateUser4OnePassword : IReturn<bool> {
        [ApiMember(Name = "id", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
        [ApiMember(Name = "password", Description = "User password", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Password { get; set; }
    }
}