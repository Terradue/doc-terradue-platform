using System;
using ServiceStack.ServiceHost;
using Terradue.Portal;
using Terradue.WebService.Model;
using ServiceStack.Common.Web;
using System.Web;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.TepQW.WebServer {

    /// <summary>
    /// Email confirmation service
    /// </summary>
    [Api("Tep-QuickWin Terradue webserver")]
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
                context.Open();
                context.Close();
                return new HttpError(System.Net.HttpStatusCode.MethodNotAllowed, new InvalidOperationException("Email already confirmed"));

                // The user is pending activation
            } catch (PendingActivationException e) {
                AuthenticationType authType = IfyWebContext.GetAuthenticationType(typeof(TokenAuthenticationType));

                // User is logged, now we confirm the email with the token
                var tokenUser = ((TokenAuthenticationType)authType).AuthenticateUser(context, request.Token);
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

                return new HttpError(System.Net.HttpStatusCode.BadRequest, new InvalidOperationException("Account does not require email confirmation"));

            } catch (PendingActivationException e) {
                AuthenticationType pwdauthType = IfyWebContext.GetAuthenticationType(typeof(PasswordAuthenticationType));
                var usr = pwdauthType.GetUserProfile(context, HttpContext.Current.Request, false);
                if (usr == null)
                    return new HttpError(System.Net.HttpStatusCode.BadRequest, new UnauthorizedAccessException("Not valid user"));

                usr.SendMail(UserMailType.Registration, true);

                return new HttpResult(new EmailConfirmationMessage(){ Status = "sent", Email = usr.Email });
            }
        }


    }

    public class EmailConfirmationMessage {
        public string Status { get; set; }

        public string Email { get; set; }
    }
}

