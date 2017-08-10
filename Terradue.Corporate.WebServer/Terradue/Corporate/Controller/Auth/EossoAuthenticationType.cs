using System;
using System.Web;
using ServiceStack.Common.Web;
using Terradue.Ldap;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {

    /// <summary>
    /// Eosso authentication type.
    /// </summary>
    public class EossoAuthenticationType : AuthenticationType {
        
        private string EossoUsername, EossoEmail, EossoOriginator;

        /// <summary>
        /// Indicates whether the authentication type depends on external identity providers.
        /// </summary>
        /// <value><c>true</c> if uses external identity provider; otherwise, <c>false</c>.</value>
        public override bool UsesExternalIdentityProvider {
            get {
                return true;
            }
        }

        /// <summary>
        /// In a derived class, checks whether a session corresponding to the current web server session exists on the
        /// external identity provider.
        /// </summary>
        /// <returns><c>true</c> if this instance is external session active the specified context request; otherwise, <c>false</c>.</returns>
        /// <param name="context">Context.</param>
        /// <param name="request">Request.</param>
        public override bool IsExternalSessionActive(IfyWebContext context, HttpRequest request){
            return true;
        }
            
        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OAuth.OAuth2AuthenticationType"/> class.
        /// </summary>
        public EossoAuthenticationType(IfyContext context) : base(context) {}

        public void SetUserInformation(string username, string email, string originator = null){
            this.EossoUsername = username;
            this.EossoEmail = email;
            this.EossoOriginator = originator;
        }

        /// <summary>
        /// Gets the user profile.
        /// </summary>
        /// <returns>The user profile.</returns>
        /// <param name="context">Context.</param>
        /// <param name="request">Request.</param>
        public override User GetUserProfile(IfyWebContext context, HttpRequest request = null, bool strict = false){
            UserT2 usr = null;
            if (string.IsNullOrEmpty(EossoUsername)) return null;

            AuthenticationType eossoAuthType = IfyWebContext.GetAuthenticationType(typeof(EossoAuthenticationType));
            var validusername = UserT2.MakeUsernameValid(EossoUsername);

            //case user exists (test with EOSSO attribute on ldap)
            var json2Ldap = new Json2LdapFactory(context);
            var lusr = json2Ldap.GetUserFromEOSSO(EossoUsername);
            if (lusr != null) {
                try {
                    usr = UserT2.FromUsername(context, lusr.Username);
                }catch(Exception e){
					//TODO: user may be on ldap but not on db
					usr = UserT2.Create(context, validusername, EossoEmail, HttpContext.Current.Session.SessionID, eossoAuthType, false, EossoUsername);
                }
            }

            //case user exists (test with email)
            if (usr == null) {
                try {
                    usr = UserT2.FromEmail(context, EossoEmail);
                } catch (Exception e) {
                    //user does not exist, we'll create it
                }
                if (usr != null) {
					usr.LoadLdapInfo();
					//if user does not have the Eosso attribute we add it
					if (string.IsNullOrEmpty(usr.EoSSO)) {
						usr.EoSSO = EossoUsername;
						usr.UpdateLdapAccount();
                    } 
                    //if user has the Eosso attribute, we check it matches
                    else {
                        if (usr.EoSSO != EossoUsername) {
                            var message = "Your email is already associated to another EO-SSO name: " + usr.EoSSO;
                            context.LogError(this, message);
                            throw new Exception(message);
                        }
                    }
                }
            }

            if (usr != null) {
                //if email is different on LDAP, we take the one from the request as reference, but user must revalidate his email
                //try {
                //    if (!string.IsNullOrEmpty(EossoEmail) && !EossoEmail.Equals(usr.Email)) {
                //        usr.Email = EossoEmail;
                //        usr.AccountStatus = AccountStatusType.PendingActivation;
                //        //update email on ldap
                //        usr.UpdateLdapAccount();
                //        //update email on db
                //        usr.Store();
                //    }
                //} catch (Exception e) { context.LogError(this, e.Message); }

				//if user is associated to Eosso Authentication Type, we set his password to the current Session Id
                try{
	                int userId = User.GetUserId(context, usr.Username, eossoAuthType);
                    if (userId != 0) {
                        context.LogDebug(this, "userId = " + userId + " ; authId = " + eossoAuthType.Id);
                        context.LogDebug(this, "Password changed for user " + usr.Username);
                        usr.ChangeLdapPassword(HttpContext.Current.Session.SessionID, null, true);
                    }
				} catch (Exception e) { context.LogError(this, e.Message); }
               
                return usr;
            }

            //case user does not exists
            context.LogDebug(this, "User " + validusername + "does not exists, we create it");
            usr = UserT2.Create(context, validusername, EossoEmail, HttpContext.Current.Session.SessionID, eossoAuthType, true, EossoUsername);

			return usr;
        }

        public override void EndExternalSession(IfyWebContext context, HttpRequest request, HttpResponse response) {
            DBCookie.DeleteDBCookies(context, HttpContext.Current.Session.SessionID);
            response.Headers[HttpHeaders.Location] = context.BaseUrl;
        }

    }
}


