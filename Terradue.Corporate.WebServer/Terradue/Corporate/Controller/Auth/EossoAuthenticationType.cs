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
            bool newUser = false;
            if (string.IsNullOrEmpty(EossoUsername)) return null;

            context.LogDebug(this, "Get user profile via EOSSO Auth");

            AuthenticationType eossoAuthType = IfyWebContext.GetAuthenticationType(typeof(EossoAuthenticationType));
            var validusername = UserT2.MakeUsernameValid(EossoUsername);

            //check if user exists on ldap with same EOSSO attribute
            var json2Ldap = new Json2LdapFactory(context);
            var lusr = json2Ldap.GetUserFromEOSSO(EossoUsername);
            if (lusr != null) {
                context.LogDebug(this, "User found on LDAP (via eosso attribute)");
                try {
                    usr = UserT2.FromUsername(context, lusr.Username);
                }catch(Exception e){
                    context.LogDebug(this, "User not found on DB -- " + e.Message);
					//TODO: user may be on ldap but not on db
                    usr = UserT2.Create(context, lusr.Username, EossoEmail, HttpContext.Current.Session.SessionID, eossoAuthType, AccountStatusType.Enabled, false, EossoUsername, EossoOriginator, false);
                    newUser = true;
                }
            }

            //check if user exists on ldap with same email
            if (usr == null) {
                lusr = json2Ldap.GetUserFromEmail(EossoEmail);
                if (lusr != null) {
                    context.LogDebug(this, "User found on LDAP (via email)");
                    //if user has the Eosso attribute, we check it matches
                    if (!string.IsNullOrEmpty(lusr.EoSSO) && lusr.EoSSO != EossoUsername) {
						var message = "Your email is already associated to another EO-SSO name: " + lusr.EoSSO;
						context.LogError(this, message);
						throw new Exception(message);
					}
                    try {
                        usr = UserT2.FromUsername(context, lusr.Username);
					} catch (Exception e) {
                        context.LogDebug(this, "User not found on DB -- " + e.Message);
                        //TODO: user may be on ldap but not on db
                        usr = UserT2.Create(context, lusr.Username, EossoEmail, HttpContext.Current.Session.SessionID, eossoAuthType, AccountStatusType.Enabled, false, EossoUsername, EossoOriginator, false);
                        newUser = true;
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

                //load the apikey
                context.LogDebug(this, "Loading apikey");
                usr.LoadApiKey();

				//if user is associated to Eosso Authentication Type, we set his password to the current Session Id
                try{
	                int userId = User.GetUserId(context, usr.Username, eossoAuthType);
                    if (userId != 0) {
                        if (!newUser) {//we change the password only if the user was not just created
                            context.LogDebug(this, "Changing Password for user " + usr.Username);
                            usr.ChangeLdapPassword(HttpContext.Current.Session.SessionID, null, true);
                        }
                        if (string.IsNullOrEmpty(usr.ApiKey)) {
                            context.LogDebug(this, "Generating ApiKey for user " + usr.Username);
                            usr.GenerateApiKey(HttpContext.Current.Session.SessionID);
                        }
                    }
				} catch (Exception e) { context.LogError(this, e.Message); }

                //if user does not have the Eosso attribute we add it
                if (string.IsNullOrEmpty(usr.EoSSO)) {
                    context.LogDebug(this, "Adding EOSSO attribute for user " + usr.Username);
                    usr.EoSSO = EossoUsername;
                    usr.UpdateLdapAccount();
                }
                context.LogDebug(this, "Returning user " + usr.Username);
                return usr;
            }

            //case user does not exists
            context.LogDebug(this, "User " + validusername + " does not exists, we create it");
            usr = UserT2.Create(context, validusername, EossoEmail, HttpContext.Current.Session.SessionID, eossoAuthType, AccountStatusType.Enabled, true, EossoUsername, EossoOriginator, false);
            context.LogDebug(this, "Returning user " + usr.Username);
			return usr;
        }

        public override void EndExternalSession(IfyWebContext context, HttpRequest request, HttpResponse response) {
            DBCookie.DeleteDBCookies(context, HttpContext.Current.Session.SessionID);
            response.Headers[HttpHeaders.Location] = context.BaseUrl;
        }

    }
}


