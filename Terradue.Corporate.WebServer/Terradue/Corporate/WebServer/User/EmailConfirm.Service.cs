using System;
using ServiceStack.ServiceHost;
using Terradue.Portal;
using Terradue.WebService.Model;
using ServiceStack.Common.Web;
using System.Web;
using Terradue.Corporate.WebServer.Common;
using Terradue.Corporate.Controller;

namespace Terradue.Corporate.WebServer {

    /// <summary>
    /// Email confirmation service
    /// </summary>
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class EmailConfirm : ServiceStack.ServiceInterface.Service {


        /// <summary>
        /// This method allows user to confirm its email adress with a token key
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(ConfirmUserEmail request) {

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            // Let's try to open context
            try {
                context.Open ();
                AuthenticationType authType = IfyWebContext.GetAuthenticationType (typeof (TokenAuthenticationType));
                // User is logged, now we confirm the email with the token
                UserT2 tokenUser = (UserT2)((TokenAuthenticationType)authType).AuthenticateUser (context, request.Token);

                //case Everest user, we must set to Explorer if user not from group Citizens
                if (tokenUser.AuthTypes != null) {
                    try{
                        foreach (var auth in tokenUser.AuthTypes) {
                            if (auth is EverestAuthenticationType) {
                                if (!EverestAuthenticationType.IsUserCitizens (context, tokenUser)) {
                                    var plan = new Plan { Role = Role.FromIdentifier (context, "plan_Explorer") };
                                    tokenUser.Upgrade (plan);
                                    break;
                                }
                            }
                        }
                    } catch (Exception e) {
                        //we donnot throw, admin will be able to update the Plan manually
                        context.LogError (this, e.Message);
                    }
                }
                context.Close();
//                return new HttpError(System.Net.HttpStatusCode.MethodNotAllowed, new InvalidOperationException("Email already confirmed"));

                // The user is pending activation
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }

            context.Close();
            return new WebResponseBool(true);

        }

        /// <summary>
        /// This method allows user to request the confirmation email
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(SendUserEmailConfirmationEmail request) {

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);

            try {
                context.Open();

                User usr = User.FromId(context, context.UserId);
                if(usr.AccountStatus != AccountStatusType.PendingActivation) return new HttpError(System.Net.HttpStatusCode.BadRequest, new UnauthorizedAccessException("Not valid user"));
                    
                usr.SendMail(UserMailType.Registration, true);
                context.Close();
                return new HttpResult(new EmailConfirmationMessage(){ Status = "sent", Email = usr.Email });

            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
        }


    }

    public class EmailConfirmationMessage {
        public string Status { get; set; }

        public string Email { get; set; }
    }
}

