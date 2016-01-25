using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.SessionState;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using Terradue.Authentication.Ldap;
using Terradue.Authentication.OAuth;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using Terradue.WebService.Model;

namespace Terradue.Corporate.WebServer {
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
	
    [Api("Tep QuickWin Terradue webserver")]
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
        public object Post(Login request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            Terradue.WebService.Model.WebUser response = null;
            Terradue.Portal.User user = null;
            try {
                context.Open();

                try {
                    user = new LdapAuthenticationType(context).Authenticate(request.username, request.password);
                } catch (Exception e1) {
//                    try{
//                        user = T2CorporateWebContext.passwordAuthenticationType.AuthenticateUser(context, request.username, request.password);
//                    }catch(Exception e){
                    throw new Exception("Wrong username or password", e1);
//                    }
                }
                response = new Terradue.WebService.Model.WebUser(user);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return response;
        }

        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Delete(Logout request) {
            T2CorporateWebContext wsContext = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                wsContext.Open();
                wsContext.EndSession();
                wsContext.Close();
            } catch (Exception e) {
                wsContext.Close();
                throw e;
            }
            return true;
        }


        public object Get(CallBack request) {

            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            Terradue.Portal.User user = null;
            try {
                context.Open();

                OAuth2AuthenticationType oauth2 = new OAuth2AuthenticationType(context);
                oauth2.GetAccessToken();
                user = oauth2.GetUserProfile(context);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return user;
        }   

        public object Get(Auth request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                OAuth2AuthenticationType oauth2 = new OAuth2AuthenticationType(context);
                oauth2.RequestAuthorization();
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return null;
        }

    }
}