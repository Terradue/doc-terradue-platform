using System;
using ServiceStack.ServiceHost;
using Terradue.Corporate.Controller;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using Terradue.Ldap;
using ServiceStack.Common.Web;
using System.Web;
using System.Net;
using Terradue.Authentication.Ldap;

namespace Terradue.Corporate.WebServer
{


    /*
participant "User" as US
participant "User Agent" as UA
participant "Login Module" as LM
participant "Client App" as CA
participant "Oauth Gateway" as OG
participant "Authorization Server" as OS <<internal>>
participant "Authentication Server" as AS <<internal>>

autonumber
US -> CA : initiate
CA -> CA : initiate user session (independant)
CA -> OG : /t2api/oauth?redirect_uri=<redirect_uri>&response_type=<response_type>&client_id=<client_id> (GET)
OG -> UA : get session ID from cookie
UA -> OG : return cookie (can be null)
OG -> OS : get authorization session (POST + query=<query> + [sub_sid=<SUB_SID>])
OS -> OG : return authorization session response
alt type = auth
OG -> CA : http_redirect (Location=/login?query=<query>&type=auth)
CA -> UA : http_redirect (Location=/login?query=<query>&type=auth)
UA -> LM : get login page (type=auth)
LM -> UA : return login page
UA -> UA : display login page
US -> UA : provide credentials
UA -> LM : submit login
LM -> OG : /t2api/oauth (POST + username=<username> + password=<password> + ajax=true)
OG -> AS : submit login with credentials
AS -> AS : validate credentials
alt ERROR
AS -> OG : ERROR
OG -> LM : http response, StatusCode=403 + error message
LM -> UA : error
UA -> UA : display error
UA -> US : goto 13 (provide credentials)
end
AS -> OG : OK + user info
OG -> OS : get user session (PUT + sid + user info)
OS -> OG : return user session
alt type = consent
OG -> LM : http response, StatusCode=200 + user session response containing type=consent
LM -> UA : consent page
UA -> UA : display consent page
US -> UA : provide consent
UA -> LM : submit consent
LM -> OG : /t2api/oauth (POST + scope=[<scope1>,<scope2>] + query=<query>)
OG -> AS : get user session (PUT + SID + scopes)
AS -> OG : response with callback + code
OG -> CA : response with callback + code
end
else type = consent
OG -> CA : http_redirect (Location=/login?query=<query>&type=consent)
CA -> UA : http_redirect (Location=/login?query=<query>&type=consent)
UA -> LM : get login page (type=consent)
LM -> OG : /t2api/oauth (POST + query=<query>)
OG -> LM : http response, StatusCode=200 + user info (including scopes)
LM -> UA : consent page
UA -> UA : display consent page
US -> UA : provide consent
UA -> LM : submit consent
LM -> OG : /t2api/oauth (POST + scope=[<scope1>,<scope2>] + query=<query>)
OG -> AS : get user session (PUT + SID + scopes)
AS -> OG : response with callback + code
OG -> CA : response with callback + code
else
OG -> CA : response with callback + code
end

== SPECIFIC TO T2 PORTAL ==
CA -> OG : /t2api/token (GET +code)
OG -> AS : submit token request with code
AS -> OG : response with token
OG -> CA : response with token
CA -> CA : store token in session
CA -> UA : response with user
UA -> UA : display user name

    */

    [Route ("/cb", "GET")]
    public class OauthCallBackRequest
    {
        [ApiMember (Name = "code", Description = "oauth code", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Code { get; set; }

        [ApiMember (Name = "state", Description = "oauth state", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string State { get; set; }

        [ApiMember (Name = "ajax", Description = "ajax", ParameterType = "path", DataType = "bool", IsRequired = true)]
        public bool ajax { get; set; }

        [ApiMember (Name = "error", Description = "error", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string error { get; set; }
    }

    [Route ("/sso/user", "GET")]
    public class GetUserFromSSORequest
    {
        [ApiMember (Name = "eosso", Description = "eosso name", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string EoSSO { get; set; }

        [ApiMember (Name = "email", Description = "email", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Email { get; set; }

        [ApiMember (Name = "token", Description = "token", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Token { get; set; }
    }

    [Route ("/sso/user", "POST")]
    public class PostUserFromSSORequest : WebUserT2
    {
        [ApiMember (Name = "eosso", Description = "eosso name", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string EoSSO { get; set; }

        [ApiMember (Name = "token", Description = "token", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Token { get; set; }

        [ApiMember (Name = "originator", Description = "system making the request", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string Originator { get; set; }

        [ApiMember (Name = "plan", Description = "plan's role at user creation", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string Plan { get; set; }

        [ApiMember (Name = "domain", Description = "plan's domain at user creation", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string Domain { get; set; }
    }

    [Route ("/logout", "GET", Summary = "logout", Notes = "Logout from the platform")]
    [Route ("/auth", "DELETE", Summary = "logout", Notes = "Logout from the platform")]
    public class OauthLogoutRequest : IReturn<String>{
		[ApiMember(Name = "ajax", Description = "ajax", ParameterType = "path", DataType = "bool", IsRequired = false)]
		public bool ajax { get; set; }

		[ApiMember(Name = "redirect_uri", Description = "Redirect uri", ParameterType = "path", DataType = "String", IsRequired = false)]
		public String redirect_uri { get; set; }
    }

    [Api ("Terradue Corporate webserver")]
    [Restrict (EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthService : ServiceStack.ServiceInterface.Service
    {

        public object Get (OauthCallBackRequest request)
        {

            var redirect = "";

            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            UserT2 user = null;
            try {
                context.Open ();
                context.LogInfo (this, string.Format ("/cb GET"));
                if (!string.IsNullOrEmpty (request.error)) {
                    context.EndSession ();
                    HttpContext.Current.Response.Redirect (context.BaseUrl, true);
                }

                Connect2IdClient client = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue ("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue ("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");
                client.RedirectUri = context.GetConfigValue ("sso-callback");
                client.AccessToken (request.Code);

                LdapAuthenticationType auth = new LdapAuthenticationType (context);
                auth.SetConnect2IdCLient (client);

                user = (UserT2)auth.GetUserProfile (context);
                context.LogDebug (this, string.Format ("Loaded user '{0}'", user.Username));
                user.LoadLdapInfo ();

                user.Store ();
                if (!user.HasGithubProfile ()) user.CreateGithubProfile ();
                redirect = context.GetConfigValue ("t2portal-welcomeEndpoint");

                //context.StartSession (auth, user); 

                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            HttpContext.Current.Response.Redirect (redirect, true);
            return null;
        }

        public object Delete (OauthLogoutRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            try {
                context.Open ();
                context.LogInfo (this, string.Format ("/auth DELETE"));
                context.EndSession ();
                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return true;
        }

        public object Get (OauthLogoutRequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            try {
                context.Open ();
                context.LogInfo (this, string.Format ("/logout GET"));
                context.EndSession ();
                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            if(request.redirect_uri != null)
			    return OAuthUtils.DoRedirect(context, request.redirect_uri, request.ajax);
            return true;
        }

        public object Get (GetUserFromSSORequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            try {
                context.Open ();
                context.LogInfo (this, string.Format ("/sso/user GET eosso='{0}',email='{1}'", request.EoSSO, request.Email));
                if (string.IsNullOrEmpty (request.Token) || !request.Token.Equals (context.GetConfigValue ("t2portal-token-usrsso"))) {
                    return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid token parameter"));
                }

                var json2Ldap = new Json2LdapFactory (context);
                LdapUser usr = null;
                if (!string.IsNullOrEmpty (request.EoSSO)) {

                    //check user with eosso attribute = eosso
                    usr = json2Ldap.GetUserFromEOSSO (request.EoSSO);
                    if (usr != null) {
                        //if email is different on LDAP, we take the one from the request as reference
                        if (!string.IsNullOrEmpty (request.Email) && !request.Email.Equals (usr.Email)) {
                            UserT2 user = UserT2.FromUsername (context, usr.Username);
                            user.Email = request.Email;
                            //update email on ldap
                            user.UpdateLdapAccount ();
                            //update email on db
                            user.Store ();
                        }
                        return usr.Username;
                    }

                    //check user with uid attribute = eosso
                    usr = json2Ldap.GetUserFromUid (request.EoSSO);
                    if (usr != null) {
                        UserT2 user = UserT2.FromUsername (context, usr.Username);
                        user.EoSSO = request.EoSSO;
                        //if emails is different on LDAP, we take the one from the request as reference
                        if (!string.IsNullOrEmpty (request.Email) && !request.Email.Equals (usr.Email)) {
                            user.Email = request.Email;
                            //update on db
                            user.Store ();
                        }
                        //update eosso/email on ldap
                        user.UpdateLdapAccount ();

                        return usr.Username;
                    }
                }

                if (!string.IsNullOrEmpty (request.Email)) {
                    //check user with email attribute = email
                    usr = json2Ldap.GetUserFromEmail (request.Email);
                    if (usr != null) {
                        //if eosso is null on LDAP or different, we take the one from the request as reference
                        if (!string.IsNullOrEmpty (request.EoSSO) && (string.IsNullOrEmpty (usr.EoSSO) || !request.EoSSO.Equals (usr.EoSSO))) {
                            UserT2 user = UserT2.FromUsername (context, usr.Username);
                            user.EoSSO = request.EoSSO;
                            //update eosso on ldap
                            user.UpdateLdapAccount ();
                        }
                        return usr.Username;
                    }
                }
                context.Close ();
            } catch (Exception e) {
                context.Close ();
                return null;
            }
            return null;
        }

        public object Post (PostUserFromSSORequest request)
        {
            T2CorporateWebContext context = new T2CorporateWebContext (PagePrivileges.EverybodyView);
            WebUserT2 result = null;
            var plan = new Plan ();
            try {
                context.Open ();
                context.LogInfo (this, string.Format ("/sso/user POST eosso='{0}',email='{1}',originator='{2}'", request.EoSSO, request.Email, request.Originator));

                //check request token
                if (string.IsNullOrEmpty (request.Token) || !request.Token.Equals (context.GetConfigValue ("t2portal-token-usrsso"))) {
                    return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid token parameter"));
                }
                //check request username
                if (string.IsNullOrEmpty (request.Username)) {
                    return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid username parameter"));
                }
                //check request password
                if (string.IsNullOrEmpty (request.Password)) {
                    return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid password parameter"));
                } else {
                    try {
                        UserT2.ValidatePassword (request.Password);
                    } catch (Exception e) {
                        return new HttpError (HttpStatusCode.BadRequest, e);
                    }
                }
                //check request eosso
                if (string.IsNullOrEmpty (request.EoSSO)) {
                    return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid eosso parameter"));
                }
                //check request email
                if (string.IsNullOrEmpty (request.Email) || !request.Email.Contains ("@")) {
                    return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid email parameter"));
                }
                //check request plan
                if (!string.IsNullOrEmpty (request.Plan)) {
                    try {
                        plan.Role = Role.FromIdentifier (context, "plan_" + request.Plan);
                    } catch (Exception) {
                        return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid plan parameter"));
                    }
                }
                //check request domain
                if (!string.IsNullOrEmpty (request.Domain)) {
                    try {
                        plan.Domain = Domain.FromIdentifier (context, request.Domain);
                    } catch (Exception) {
                        return new HttpError (HttpStatusCode.BadRequest, new Exception ("Invalid domain parameter"));
                    }
                } else { 
                    plan.Domain = Domain.FromIdentifier (context, "terradue");
                }

                //check if email is already used
                try {
                    UserT2.FromEmail (context, request.Email);
                    throw new Exception ("Sorry, this email is already used.");
                } catch (Exception) { }

                var validusername = UserT2.MakeUsernameValid (request.Username);

                var json2Ldap = new Json2LdapFactory (context);
                if (json2Ldap.GetUserFromEmail (request.Email) != null) throw new Exception ("Sorry, this email is already used.");
                if (json2Ldap.GetUserFromEOSSO (validusername) != null) throw new Exception ("Sorry, this username is already used.");
                if (json2Ldap.GetUserFromUid (validusername) != null) {
                    var exists = true;
                    int i = 1;
                    while (exists && i < 100) {
                        var uname = string.Format ("{0}{1}", validusername, i);
                        if (json2Ldap.GetUserFromUid (uname) == null) {
                            exists = false;
                            request.Username = uname;//set request because we then create the user entity from request
                        } else {
                            i++;
                        }
                    }
                    if (i == 99) throw new Exception ("Sorry, we were not able to find a valid username");
                }

                AuthenticationType AuthType = IfyWebContext.GetAuthenticationType (typeof (LdapAuthenticationType));

                UserT2 user = request.ToEntity (context, new UserT2 (context));
                user.NeedsEmailConfirmation = false;
                //we assume email was already validated on the system making the request, so user is automatically enabled
                user.AccountStatus = AccountStatusType.Enabled;
                user.Level = UserLevel.User;
                user.PasswordAuthenticationAllowed = true;

                user.CreateLdapAccount (request.Password);

                user.EoSSO = request.EoSSO;
                user.UpdateLdapAccount ();
                user.RegistrationOrigin = request.Originator;

                //need to use admin rigths to create user
                var currentContextAccessLevel = context.AccessLevel;
                var currentUserAccessLevel = user.AccessLevel;
                context.AccessLevel = EntityAccessLevel.Administrator;
                user.AccessLevel = EntityAccessLevel.Administrator;
                user.Store ();
                context.AccessLevel = currentContextAccessLevel;
                user.AccessLevel = currentUserAccessLevel;

                user.LinkToAuthenticationProvider (AuthType, user.Username);
                user.CreateGithubProfile ();
                try {
                    user.SendMail (UserMailType.Registration, true);
                } catch (Exception) { }

                try {
                    var subject = "[T2 Portal] - User registration on Terradue Portal";
                    var originator = request.Originator != null ? "\nThe request was performed from " + request.Originator : "";
                    var body = string.Format ("This is an automatic email to notify that an account has been automatically created on Terradue Corporate Portal for the user {0} ({1}).{2}", user.Username, user.Email, originator);
                    context.SendMail (context.GetConfigValue ("SmtpUsername"), context.GetConfigValue ("SmtpUsername"), subject, body);
                } catch (Exception) {
                    //we dont want to send an error if mail was not sent
                }

                //Set plan
                if (!string.IsNullOrEmpty (request.Plan)){
                    user.Upgrade (plan);
                }

                //TODO: log user created from TEP

                result = new WebUserT2 (user);

                context.Close ();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }
    }
}

