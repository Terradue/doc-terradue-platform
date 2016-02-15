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
using Terradue.Authentication.Ldap;
using Terradue.Authentication.OAuth;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserService : ServiceStack.ServiceInterface.Service {
        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(GetUserT2 request) {
            WebUserT2 result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, request.Id);
                result = new WebUserT2(user);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Get the current user.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns>the current user</returns>
        public object Get(GetCurrentUserT2 request) {
            WebUserT2 result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            context.ConsoleDebug = true;
            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, context.UserId);
                result = new WebUserT2(user);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Get the list of all users.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns>the users</returns>
        public object Get(GetUsers request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            List<WebUserT2> result = new List<WebUserT2>();
            try {
                context.Open();

                EntityList<UserT2> users = new EntityList<UserT2>(context);
                users.Load();
                foreach(UserT2 u in users) result.Add(new WebUserT2(u));

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Update the specified user.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns>the user</returns>
        public object Put(UpdateUserT2 request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebUserT2 result;
            try {
                context.Open();
				UserT2 user = (request.Id == 0 ? null : UserT2.FromId(context, request.Id));
                user = request.ToEntity(context, user);
                user.Store();

                //update the Ldap account with the modifications
                user.UpdateLdapAccount();

                result = new WebUserT2(user);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(CreateUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebUserT2 result;
            try{
                context.Open();
				UserT2 user = (request.Id == 0 ? null : UserT2.FromId(context, request.Id));
				user = request.ToEntity(context, user);
                if(request.Id != 0 && context.UserLevel == UserLevel.Administrator){
                    user.AccountStatus = AccountStatusType.Enabled;
                }
                else{
                    user.AccountStatus = AccountStatusType.PendingActivation;
                }

                user.IsNormalAccount = true;
                user.Level = UserLevel.User;

                user.Store();
                user.StorePassword(request.Password);

                result = new WebUserT2(user);
                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(RegisterUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            WebUserT2 result;
            try{
                context.Open();

                //validate catcha
                ValidateCaptcha(context.GetConfigValue("reCaptcha-secret"), request.captchaValue);

                bool exists = false;
                try{
                    User.FromUsername(context, request.Email);
                    exists = true;
                }catch(Exception){}

                try{
                    var ldapauth = new Terradue.Ldap.LdapAuthClient(context.GetConfigValue("ldapauth-baseurl"), context.GetConfigValue("ldap-port"));
                    var usr = ldapauth.GetUser(request.Email);
                    if(usr != null) exists = true;
                }catch(Exception){}

                if(exists) throw new Exception("User already exists");

                AuthenticationType AuthType = IfyWebContext.GetAuthenticationType(typeof(OAuth2AuthenticationType));

                UserT2 user = request.ToEntity(context, new UserT2(context));
                user.Username = user.Email;
                user.NeedsEmailConfirmation = false;
                user.AccountStatus = AccountStatusType.PendingActivation;
                user.Level = UserLevel.User;
                user.PasswordAuthenticationAllowed = true;
                user.Store();

                try{
                    user.CreateLdapAccount(request.Password);
                    user.LinkToAuthenticationProvider(AuthType, user.Username);
                }catch(Exception e){
//                    user.Delete();
//                    user.DeleteLdapAccount();
                    throw e;
                }

                //we dont want to send an error if mail was not sent
                //user can still have it resent from the portal
                try{
                    user.SendMail(UserMailType.Registration, true);
                }catch(Exception){}
                    
                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public object Post(UpgradeUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebUserT2 result;
            try{
                context.Open();
                if(request.Id == 0) throw new Exception("Wrong user Id");

                UserT2 user = UserT2.FromId(context, request.Id);

                if(context.UserLevel == UserLevel.Administrator){
                    user.Upgrade((PlanType)request.Level);
                } else {
                    if(context.UserId != request.Id) throw new Exception("Wrong user Id");

                    string plan = "";
                    switch(request.Level){
                        case (int)PlanType.DEVELOPER:
                            plan = "Developer";
                            break;
                        case (int)PlanType.INTEGRATOR:
                            plan = "Integrator";
                            break;
                        case (int)PlanType.PRODUCER:
                            plan = "Provider";
                            break;
                        default:
                            plan = "Developer";
                            break;
                    }

                    string subject = context.GetConfigValue("EmailSupportUpgradeSubject");
                    subject = subject.Replace("$(PORTAL)", context.GetConfigValue("SiteName"));

                    string body = context.GetConfigValue("EmailSupportUpgradeBody");
                    body = body.Replace("$(USERNAME)", user.Username);
                    body = body.Replace("$(PLAN)", plan);
                    body = body.Replace("$(MESSAGE)", request.Message);

                    context.SendMail(user.Email, context.GetConfigValue("MailSenderAddress"), subject, body); 
                }
                result = new WebUserT2(new UserT2(context, user));
                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Delete the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Delete(DeleteUser request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, request.Id);
                if (context.UserLevel == UserLevel.Administrator){
                    user.DeleteLdapAccount();
                    user.Delete();
                }
                else throw new UnauthorizedAccessException(CustomErrorMessages.ADMINISTRATOR_ONLY_ACTION);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public object Put(UserResetPassword request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                string subject = context.GetConfigValue("EmailSupportResetPasswordSubject");
                subject = subject.Replace("$(PORTAL)", context.GetConfigValue("SiteName"));

                string body = context.GetConfigValue("EmailSupportResetPasswordBody");
                body = body.Replace("$(USERNAME)", request.Username);
                body = body.Replace("$(PORTAL)", context.GetConfigValue("SiteName"));

                context.SendMail(request.Username, context.GetConfigValue("MailSenderAddress"), subject, body); 

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public void ValidateCaptcha(string secret, string response){
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            string json = "{" +
                "\"secret\":\"" + secret+"\"," +
                "\"response\":\"" + response+"\"," +
                "}";
            
            using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    string result = streamReader.ReadToEnd();
                    try{
                        CaptchaResponse captchaResponse = JsonSerializer.DeserializeFromString<CaptchaResponse>(result);
                        if (!captchaResponse.Success)
                        {
                            if (captchaResponse.ErrorCodes.Count <= 0) throw new Exception("Error occured. Please try again.");

                            var error = captchaResponse.ErrorCodes[0].ToLower();
                            switch (error)
                            {
                                case ("missing-input-secret"):
                                    throw new Exception("The secret parameter is missing.");
                                    break;
                                case ("invalid-input-secret"):
                                    throw new Exception("The secret parameter is invalid or malformed.");
                                    break;

                                case ("missing-input-response"):
                                    throw new Exception("The response parameter is missing.");
                                    break;
                                case ("invalid-input-response"):
                                    throw new Exception("The response parameter is invalid or malformed.");
                                    break;

                                default:
                                    throw new Exception("Error occured. Please try again");
                                    break;
                            }
                        }
                    }catch(Exception e){
                        throw new Exception("Error occured. Please try again.");
                    }
                }
            }
        }

    }
        
    [DataContract]
    public class CaptchaResponse{
        [DataMember(Name="success")]
        public bool Success { get; set; }

        [DataMember(Name="error-codes")]
        public List<string> ErrorCodes { get; set; }
    }



}

