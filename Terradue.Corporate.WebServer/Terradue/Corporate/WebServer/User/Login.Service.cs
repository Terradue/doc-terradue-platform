using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.SessionState;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using Terradue.Authentication.OAuth;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using Terradue.WebService.Model;
using Terradue.Corporate.Controller;
using Terradue.Ldap;

namespace Terradue.Corporate.WebServer {
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
	
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
           EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
	/// Login service. Used to log into the system (replacing UMSSO for testing)
	/// </summary>
	public class LoginService : ServiceStack.ServiceInterface.Service {
        
        /// <summary>
        /// Username/password login
        /// </summary>
        /// <param name="request">Request.</param>
//        public object Post(Login request) {
//            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
//            Terradue.WebService.Model.WebUser response = null;
//            Terradue.Portal.User user = null;
//            try {
//                context.Open();
//
//                try {
//                    user = new LdapAuthenticationType(context).Authenticate(request.username, request.password);
//                } catch (Exception e1) {
////                    try{
////                        user = T2CorporateWebContext.passwordAuthenticationType.AuthenticateUser(context, request.username, request.password);
////                    }catch(Exception e){
//                    throw new Exception("Wrong username or password", e1);
////                    }
//                }
//                response = new Terradue.WebService.Model.WebUser(user);
//
//                context.Close();
//            } catch (Exception e) {
//                context.Close();
//                throw e;
//            }
//            return response;
//        }


    }
}