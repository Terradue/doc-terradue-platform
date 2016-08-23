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
using Terradue.Authentication.OAuth;
using Terradue.Ldap;


/*
 * Sequence diagram for User registration
participant "GEP" as GP
participant "User" as US
participant "T2Portal" as T2
participant "T2portal Database" as DB
participant "LDAP" as LP
participant "EO-SSO" as SS
participant "Zendesk" as ZK
participant "Cloud Controller" as CC

== DEFAULT REGISTRATION ==
US -> T2 : <b>Sign up</b>\n(email, password)
T2 -> DB : <b>Create User</b>\nUsername=email\nEmail=email\n
T2 -> LP : <b>Create User</b>\nuid=email\nemail=email\npassword=password
T2 -> US : sends a confirmation email
US -> T2 : confirm email
T2 -> DB : Update user status
T2 -> US : redirect to profile page
T2 -> US : auto propose username on names completion
US -> T2 : <b>Edit Profile</b>\n(firstname, lastname, username)
T2 -> DB : <b>Update User</b>\nUsername=username\nFirstName=firstname\nLastName=lastname\nEmail=email\n
T2 -> LP : <b>Update User</b>\nuid=username\nname=firstname\ngiven_name=lastname\nemail=email\n

== REGISTRATION FROM TEP ==
US -> GP : Login with EO-SSO Account
GP -> SS : login + (callback_url = /login)
US -> SS : username / password
SS -> GP : /login + username / email

alt exists ldap entry with eosso=eosso username
GP -> US : wants to setup Terradue Account ? (profile page)
US -> GP : Setup Account
GP -> T2 : redirects to (GET) /oauth + query including eosso
T2 -> T2 : auto login (client is trusted) + maj infos user if necessary
else
alt exists ldap entry with email=email
GP -> US : wants to setup Terradue Account ? (profile page)
US -> GP : Setup Account
GP -> T2 : redirects to (GET) /oauth + query including eosso
T2 -> T2 : auto login (client is trusted) + maj eosso username + maj infos user if necessary
else
T2 -> T2 : auto create user + maj eosso username + maj infos user if necessary + random password
T2 -> US : redirects to profile page
US -> T2 : <b>Edit Profile</b>\n(firstname, lastname, username) + GEP callback url
T2 -> DB : <b>Update User</b>\nUsername=username\nFirstName=firstname\nLastName=lastname\nEmail=email\n
T2 -> LP : <b>Update User</b>\nuid=username\nname=firstname\ngiven_name=lastname\nemail=email\n
end
end
T2 -> GP : returns username
GP -> GP : save username for user

== LOGIN USING EO-SSO ==
US -> T2 : login with EO-SSO
T2 -> GP : /t2login
GP -> SS : login + (callback_url = /t2login)
US -> SS : username / password
SS -> GP : /t2login + username / email
GP -> T2 : returns json (including eosso username, email, first name, last name) (or JWT ?)
alt exists ldap entry with eosso=eosso username
T2 -> T2 : auto login (client is trusted) + maj infos user if necessary
else
alt exists ldap entry with email=email
T2 -> T2 : auto login (client is trusted) + maj eosso username + maj infos user if necessary
else
T2 -> T2 : auto create user + maj eosso username + maj infos user if necessary + random password
end
end
T2 -> US : profile page
US -> T2 : <b>Edit Profile</b>\n(firstname, lastname, username)
T2 -> DB : <b>Update User</b>\nUsername=username\nFirstName=firstname\nLastName=lastname\nEmail=email\n
T2 -> LP : <b>Update User</b>\nuid=username\nname=firstname\ngiven_name=lastname\nemail=email\n
 */
using System.Linq;
using System.Web;
using Terradue.Github;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserService : ServiceStack.ServiceInterface.Service {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
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
                result = new WebUserT2(user, false);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(GetUserNameT2 request) {
            WebUserT2 result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                UserT2 user = UserT2.FromUsername(context, request.Username);
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
                log.InfoFormat("Get current user {0}",user.Username);
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
            var result = new List<WebUserT2>();
            try {
                context.Open();

                EntityList<UserT2> users = new EntityList<UserT2>(context);
                users.Load();
                foreach(UserT2 u in users) result.Add(new WebUserT2(u, true));

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
                if(request.Id==0) throw new Exception("Id = 0");
				UserT2 user = (request.Id == 0 ? null : UserT2.FromId(context, request.Id));
                bool newusername = (user.Username == user.Email);
                user = request.ToEntity(context, user);

                //update the Ldap uid
                if(newusername){
                    user.UpdateUsername();
                }

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
//        public object Post(CreateUserT2 request)
//        {
//            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
//            WebUserT2 result;
//            try{
//                context.Open();
//				UserT2 user = (request.Id == 0 ? null : UserT2.FromId(context, request.Id));
//				user = request.ToEntity(context, user);
//                if(request.Id != 0 && context.UserLevel == UserLevel.Administrator){
//                    user.AccountStatus = AccountStatusType.Enabled;
//                }
//                else{
//                    user.AccountStatus = AccountStatusType.PendingActivation;
//                }
//
//                user.IsNormalAccount = true;
//                user.Level = UserLevel.User;
//
//                user.Store();
//                user.StorePassword(request.Password);
//
//                result = new WebUserT2(user);
//                context.Close ();
//            }catch(Exception e) {
//                context.Close ();
//                throw e;
//            }
//            return result;
//        }

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
                    UserT2.FromEmail(context, request.Email);
                    exists = true;
                }catch(Exception){}

                if(exists) throw new Exception("Sorry, this email is already used.");

                try{
                    var json2Ldap = new Json2LdapFactory(context);
                    if (json2Ldap.GetUserFromEmail(request.Email) != null) exists = true;
                }catch(Exception){}

                if(exists) throw new Exception("Sorry, this email is already used.");
                    
                AuthenticationType AuthType = IfyWebContext.GetAuthenticationType(typeof(OAuth2AuthenticationType));

                UserT2 user = request.ToEntity(context, new UserT2(context));
                user.Username = user.Email;
                user.NeedsEmailConfirmation = false;
                user.AccountStatus = AccountStatusType.PendingActivation;
                user.Level = UserLevel.User;
                user.PasswordAuthenticationAllowed = true;

                try{
                    Connect2IdClient client = new Connect2IdClient(context.GetConfigValue("sso-configUrl"));
                    client.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                    client.SSOApiClient = context.GetConfigValue("sso-clientId");
                    client.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                    client.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");
                    var client_id = context.GetConfigValue("sso-clientId");
                    var response_type = "code";
                    var nonce = Guid.NewGuid().ToString();
                    var scope = context.GetConfigValue("sso-scopes").Replace(","," ");
                    var state = Guid.NewGuid().ToString();
                    var redirect_uri = HttpUtility.UrlEncode(context.GetConfigValue("sso-callback"));

                    var query = string.Format("response_type={0}&scope={1}&client_id={2}&state={3}&redirect_uri={4}&nonce={5}",
                                              response_type, scope, client_id, state, redirect_uri, nonce);
                    
                    client.SID = null;
                    client.SUB_SID = null;
                    client.OAUTHTOKEN_ACCESS = null;
                    client.OAUTHTOKEN_REFRESH = null;

                    user.CreateLdapAccount(request.Password);
                    user.Store();
                    user.LinkToAuthenticationProvider(AuthType, user.Username);
                    user.CreateGithubProfile();

                    //create new SID
                    var oauthsession = client.AuthzSession(new OauthAuthzPostSessionRequest { query = query }, true);

                    client.SID = oauthsession.sid;
                }catch(Exception e){
//                    user.Delete();
//                    user.DeleteLdapAccount();
                    throw e;
                }

                try{
                    user.SendMail(UserMailType.Registration, true);
                }catch(Exception){
                    //we dont want to send an error if mail was not sent
                    //user can still have it resent from the portal
                }

                try{
                    var subject = "[T2 Portal] - User registration on Terradue Portal";
                    var body = string.Format("This is an automatic email to notify that the user {0} registered on Terradue Portal.", user.Username);
                    context.SendMail(context.GetConfigValue("SmtpUsername"),context.GetConfigValue("SmtpUsername"),subject,body);
                }catch(Exception){
                    //we dont want to send an error if mail was not sent
                }

                try{
                    using (var service = base.ResolveService<OAuthGatewayService>()) { 
                        var client_id = context.GetConfigValue("sso-clientId");
                        var response_type = "code";
                        var scope = context.GetConfigValue("sso-scopes").Replace(","," ");
                        var state = Guid.NewGuid().ToString();
                        var redirect_uri = context.GetConfigValue("sso-callback");
                        var query = string.Format("response_type={0}&scope={1}&client_id={2}&state={3}&redirect_uri={4}",
                                          response_type, scope, client_id, state, redirect_uri);
                        var response = service.Post(new OauthLoginRequest{
                            username = request.Email,
                            password = request.Password,
                            ajax = true,
                            query = query,
                            autoconsent = true
                        });
                        return response;
                    }; 
                }catch(Exception e){
                    throw new Exception("Your account has been successfully created, but there has been a problem to log you in. Please try again to sign-in.", e);
                }

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

                Plan plan = Plan.FromName(context, request.Plan);

                if(context.UserLevel == UserLevel.Administrator){
                    user.Upgrade(plan);
                } else {
                    if(context.UserId != request.Id) throw new Exception("Wrong user Id");

                    string subject = context.GetConfigValue("EmailSupportUpgradeSubject");
                    subject = subject.Replace("$(PORTAL)", context.GetConfigValue("SiteName"));

                    string body = context.GetConfigValue("EmailSupportUpgradeBody");
                    body = body.Replace("$(USERNAME)", user.Username);
                    body = body.Replace("$(PLAN)", plan.Name);
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
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            try {
                context.Open();
                Json2LdapFactory ldapfactory = new Json2LdapFactory(context);

                UserT2 user = UserT2.FromId(context, request.Id);
                if(user.GetCloudUser() != null) user.DeleteCloudAccount();
                if(ldapfactory.UserExists(user.Username)) user.DeleteLdapAccount();
                user.Delete();

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public object Post(ResetPassword request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                UserT2 user;
                try{
                    user = UserT2.FromUsername(context, request.Username);
                }catch(Exception){
                    try{
                        user = UserT2.FromEmail(context, request.Username);
                    }catch(Exception){
                            return new WebResponseBool(true);
                        }
                }

                //send email to user with new token
                var token = user.GetToken();

                string subject = context.GetConfigValue("EmailSupportResetPasswordSubject");
                subject = subject.Replace("$(PORTAL)", context.GetConfigValue("SiteName"));

                string body = context.GetConfigValue("EmailSupportResetPasswordBody");
                body = body.Replace("$(USERNAME)", request.Username);
                body = body.Replace("$(PORTAL)", context.GetConfigValue("SiteName"));
                body = body.Replace("$(LINK)", context.BaseUrl + "/portal/passwordreset?token=" + token);

                context.SendMail(context.GetConfigValue("MailSenderAddress"), user.Email, subject, body);

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

                if(string.IsNullOrEmpty(request.Password)) throw new Exception("Password is empty");

                UserT2 user = null;
                try {
                    user = UserT2.FromUsername(context, request.Username);
                }catch(Exception){
                    try {
                        user = UserT2.FromEmail(context, request.Username);
                    }catch(Exception e){
                        throw new Exception("User not found");
                    }
                }

                user.ValidateActivationToken(request.Token);

                try{
                    user.ChangeLdapPassword(request.Password, null, true);
                }catch(Exception e){
                    throw new Exception("Unable to change password", e);    
                }
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public object Put(UserUpdatePassword request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();

                if(string.IsNullOrEmpty(request.NewPassword)) throw new Exception("Password is empty");

                UserT2 user = UserT2.FromId(context, context.UserId);

                try{
                    user.ChangeLdapPassword(request.NewPassword, request.OldPassword);
                }catch(Exception e){
                    throw new Exception("Unable to change password", e);    
                }
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public object Put(UpdateEmailUserT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebUserT2 result = null;
            try {
                context.Open();

                UserT2 user = UserT2.FromId(context, context.UserId);
                user.ValidateNewEmail(request.Email);
                if(user.Email == request.Email) throw new Exception("You must choose an email different from the current one.");

                user.Email = request.Email;
                user.AccountStatus = AccountStatusType.PendingActivation;

                //update on ldap
                user.UpdateLdapAccount();

                //update on db
                user.Store();

                //we dont want to send an error if mail was not sent
                //user can still have it resent from the portal
                try{
                    user.SendMail(UserMailType.Registration, true);
                }catch(Exception){}

                result = new WebUserT2(user);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(GetAvailableLdapUsernameT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            bool result = true;
            try {
                context.Open();

                if(string.IsNullOrEmpty(request.username)) throw new Exception("username is empty");
                UserT2.ValidateUsername(request.username);
                Json2LdapFactory ldapfactory = new Json2LdapFactory(context);
                result = !ldapfactory.UserExists(request.username);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
			return result;
        }

        public object Get(GetPrivateUserInfoT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try{
                context.Open();

                if(string.IsNullOrEmpty(request.request)){
                    return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid request parameter"));
                }
                if(string.IsNullOrEmpty(request.token) || !request.token.Equals(context.GetConfigValue("t2-safe-token"))){
                    return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid token parameter"));
                }
                if(string.IsNullOrEmpty(request.username)){
                    return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid username parameter"));
                }

                UserT2 user = null;
                try{
                    user = UserT2.FromUsername(context, request.username);
                }catch(Exception e){
                    user = new UserT2(context);
                }

                GithubProfile githubProfile = null;

                switch(request.request){
                    case "sshPublicKey":
                        user.PublicLoadSshPubKey(request.username);
                        return new HttpResult(user.PublicKey);
                        break;
                    case "s3":
                        break;
                    case "githubUsername":
                        githubProfile = GithubProfile.FromId(context, user.Id);
                        return new HttpResult(githubProfile.Name);
                        break;
                    case "githubToken":
                        githubProfile = GithubProfile.FromId(context, user.Id);
                        return new HttpResult(githubProfile.Token);
                        break;
                    case "githubEmail":
                        githubProfile = GithubProfile.FromId(context, user.Id);
                        return new HttpResult(githubProfile.Email);
                        break;
                    case "redmineApiKey":
                        /*
                         * string sql = String.Format ("SELECT apikey FROM usr_redmine WHERE id_usr={0};",this.Id);
            IDbConnection dbConnection = context.GetDbConnection();
            IDataReader reader = context.GetQueryResult(sql, dbConnection);

            if (!reader.Read()) {
                RedmineKey = null;
                reader.Close();
                return;
            }
            RedmineKey = reader.GetString (0);
            context.CloseQueryResult(reader, dbConnection);
                        */
                        break;
                    default:
                        break;
                }

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
                return new HttpResult();
        }

        public object Get(GetLdapUsers request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            List<WebUserT2> users = new List<WebUserT2>();
            try {
                context.Open();

                Json2LdapFactory ldapfactory = new Json2LdapFactory(context);
                var ldapusers = ldapfactory.GetUsersFromLdap();

                foreach(var ldapuser in ldapusers){
                    try{
                        users.Add(new WebUserT2(new UserT2(context, ldapuser), true));
                    }catch(Exception e){
                        var ldapu = new UserT2(context);
                        ldapu.Username = ldapuser.Username;
                        ldapu.Email = ldapuser.Email;
                        ldapu.FirstName = ldapuser.FirstName;
                        ldapu.LastName = ldapuser.LastName;
                        users.Add(new WebUserT2(ldapu, true));
                    }
                }

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return users;
        }

        public object Put(ReGenerateApiKeyUserT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            string result;
            try{
                context.Open();
                UserT2 user = UserT2.FromId(context, context.UserId);
                log.InfoFormat("Generate API Key for user {0}", user.Username);

                //authenticate user
                try{
                    var j2ldapclient = new LdapAuthClient(context.GetConfigValue("ldap-authEndpoint"));
                    var usr = j2ldapclient.Authenticate(user.Identifier, request.password, context.GetConfigValue("ldap-apikey"));
                }catch(Exception e){
                    log.ErrorFormat("Error during API Key creation - {0} - {1}", e.Message, e.StackTrace);
                    throw new Exception("Invalid password");
                }

                user.GenerateApiKey();
                log.InfoFormat("API Key created locally");
                user.UpdateLdapAccount();
                log.InfoFormat("API Key saved on ldap");

                //sanity check on the domain
                switch (user.Plan.Name) {
                    case Plan.NONE:
                        break;
                    case Plan.TRIAL:
                        break;
                    case Plan.EXPLORER:
                    case Plan.SCALER:
                    case Plan.PREMIUM:
                        if (!user.HasLdapDomain()) user.CreateLdapDomain();
                        break;
                    default:
                        break;
                }

                result = user.ApiKey;

                context.Close ();
            }catch(Exception e) {
                log.ErrorFormat("Error during api key creation - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return new WebResponseString(result);
        }

        public object Delete(DeleteApiKeyUserT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try{
                context.Open();
                UserT2 user = UserT2.FromId(context, context.UserId);
                log.InfoFormat("Revoke API Key for user {0}", user.Username);

                //authenticate user
                try{
                    var j2ldapclient = new LdapAuthClient(context.GetConfigValue("ldap-authEndpoint"));
                    var usr = j2ldapclient.Authenticate(user.Identifier, request.password, context.GetConfigValue("ldap-apikey"));
                }catch(Exception e){
                    log.ErrorFormat("Error during API Key removal - {0} - {1}", e.Message, e.StackTrace);
                    throw new Exception("Invalid password");
                }

                user.RevokeApiKey(request.password);
                log.InfoFormat("API Key deleted");

                context.Close ();
            }catch(Exception e) {
                log.ErrorFormat("Error during api key removal - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return new WebResponseBool(true);
        }

        public object Get(ValidateApiKeyUserT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            bool result;
            try{
                context.Open();
                UserT2 user = UserT2.FromUsername(context, request.username);
                log.InfoFormat("Validate API Key for user {0}", user.Username);

                user.LoadLdapInfo();
                if(user.ApiKey != null && user.ApiKey.Equals(request.key)){
                    result = true;
                    log.InfoFormat("API key is valid");
                } else {
                    result = false;
                    log.InfoFormat("API key is not valid");
                }

                context.Close ();
            }catch(Exception e) {
                log.ErrorFormat("Error during api key validation - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return new WebResponseBool(result);
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

