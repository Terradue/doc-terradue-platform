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
    public class OneUserService : ServiceStack.ServiceInterface.Service {

        public object Get(GetOneUsers request) {
            List<WebOneUser> result = new List<WebOneUser>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            try {
                context.Open();
                OneClient one = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                USER_POOL pool = one.UserGetPoolInfo();
                foreach(object u in pool.Items){
                    if(u is USER_POOLUSER){
                        USER_POOLUSER oneuser = u as USER_POOLUSER;
                        WebOneUser wu = new WebOneUser{ Id = oneuser.ID, Name = oneuser.NAME, Password = oneuser.PASSWORD, AuthDriver = oneuser.AUTH_DRIVER};
                        result.Add(wu);
                    }
                }
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(GetOneUser request) {
            WebOneUser result = null;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                OneClient one = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                USER oneuser = one.UserGetInfo(request.Id);
                result = new WebOneUser{ Id = oneuser.ID, Name = oneuser.NAME, Password = oneuser.PASSWORD, AuthDriver = oneuser.AUTH_DRIVER};

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(GetOneCurrentUser request) {
            WebOneUser result = new WebOneUser();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                User user = User.FromId(context, context.UserId);
                OneClient one = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                USER_POOL pool = one.UserGetPoolInfo();
                foreach(object u in pool.Items){
                    if(u is USER_POOLUSER){
                        USER_POOLUSER oneuser = u as USER_POOLUSER;
                        if(oneuser.NAME == user.Username){
                            result = new WebOneUser{ Id = oneuser.ID, Name = oneuser.NAME, Password = oneuser.PASSWORD, AuthDriver = oneuser.AUTH_DRIVER};
                            break;
                        }
                    }
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Put(UpdateOneUser request) {
            bool result;
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                OneClient one = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                result = one.UserUpdatePassword(Int32.Parse(request.Id), request.Password);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Post(CreateOneUser request){
            WebOneUser result;
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                User user = User.FromId(context, context.UserId);
                OneClient one = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
                int id = one.UserAllocate(user.Username, request.Password, (request.AuthDriver == null || request.AuthDriver == "" ? "x509" : request.AuthDriver));
                USER oneuser = one.UserGetInfo(id);
                result = new WebOneUser{ Id = oneuser.ID, Name = oneuser.NAME, Password = oneuser.PASSWORD, AuthDriver = oneuser.AUTH_DRIVER};
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

    }
}