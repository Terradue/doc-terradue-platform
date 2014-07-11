using System;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using Terradue.Portal;
using System.Web.SessionState;
using System.Diagnostics;
using Terradue.Corporate.WebServer.Common;
using System.Web;
using Terradue.OpenId;

namespace Terradue.Corporate.WebServer.Services
{
	//-------------------------------------------------------------------------------------------------------------------------
	//-------------------------------------------------------------------------------------------------------------------------
	//-------------------------------------------------------------------------------------------------------------------------
	
    [Api("Tep QuickWin Terradue webserver")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
	          EndpointAttributes.Secure   | EndpointAttributes.External | EndpointAttributes.Json)]
	/// <summary>
	/// Login service. Used to log into the system (replacing UMSSO for testing)
	/// </summary>
	public class LoginService : ServiceStack.ServiceInterface.Service
	{

        /// <summary>
        /// OpenId login
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(OpenIdLogin request) 
        {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            Terradue.WebService.Model.WebUser response = null;
            OpenIdAuthenticationType openId;
            User user = null;
            try{
                try{
                    context.Open();
                    if (context.IsUserIdentified) user = User.FromId(context, context.UserId);
                    else {
                        openId = new OpenIdAuthenticationType(context);
                        string url = "";
                        if(request.provider != null){
                            switch(request.provider.ToLower()){
                                case "googleopenid":
                                    url = context.GetConfigValue("OpenIdOp-Google");
                                    break;
                                case "t2openid":
                                default:
                                    url = context.GetConfigValue("OpenIdOp-Terradue");
                                    break;
                            }
                        }
                        openId.Authenticate(url);
                    }
                }catch(UnauthorizedAccessException e){
                    throw e;
                }
                context.Redirect(request.url);
                response = new Terradue.WebService.Model.WebUser(user);
                context.Close();
            }
            catch (Exception e){
                context.Close();
                throw e;
            }
            return response;
        }
		
        /// <summary>
        /// Username/password login
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(Login request) 
		{
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            Terradue.WebService.Model.WebUser response = null;
            Terradue.Portal.User user = null;
			try{
                context.Open();

                user = T2CorporateWebContext.passwordAuthenticationType.AuthenticateUser(context, request.username, request.password);
                response = new Terradue.WebService.Model.WebUser(user);

				context.Close();
			}
			catch (Exception e){
				context.Close();
                throw e;
			}
            return response;
		}

		/// <summary>
		/// Get the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
        public object Delete(Logout request) 
		{
            T2CorporateWebContext wsContext = new T2CorporateWebContext(PagePrivileges.EverybodyView);
			try{
				wsContext.Open();
				wsContext.LogoutUser();
				wsContext.Close();
			}
			catch (Exception e){
				wsContext.Close();
                throw e;
			}
            return true;
		}
	}
}
