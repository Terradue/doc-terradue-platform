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
using System.Net;
using System.IO;
using ServiceStack.Text;
using System.Runtime.Serialization;
using ServiceStack.Common.Web;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserSafeService : ServiceStack.ServiceInterface.Service {
       
        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(CreateSafeUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebSafe result;
            try{
                context.Open();

                UserT2 user = UserT2.FromId(context, context.UserId);
                user.CreateSafe(request.password);
                result = new WebSafe();
                result.PublicKey = user.GetPublicKey();
                result.PrivateKey = user.GetPrivateKey(request.password);

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Put(GetSafeUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebSafe result;
            try{
                context.Open();

                UserT2 user = UserT2.FromId(context, context.UserId);
                if(user.HasSafe()){
                    result = new WebSafe();
                    result.PublicKey = user.GetPublicKey();
                    result.PrivateKey = user.GetPrivateKey(request.password);
                } else throw new Exception("Safe has not yet been created");

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

    }
  
}

