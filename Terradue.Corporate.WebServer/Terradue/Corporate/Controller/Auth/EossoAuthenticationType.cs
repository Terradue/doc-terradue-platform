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
        
        private string Username, Email;

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

        public void SetUserInformation(string username, string email){
            this.Username = username;
            this.Email = email;
        }

        /// <summary>
        /// Gets the user profile.
        /// </summary>
        /// <returns>The user profile.</returns>
        /// <param name="context">Context.</param>
        /// <param name="request">Request.</param>
        public override User GetUserProfile(IfyWebContext context, HttpRequest request = null, bool strict = false){
            UserT2 usr;
            AuthenticationType authType = IfyWebContext.GetAuthenticationType(typeof(EossoAuthenticationType));
            try{
				//case user exists
                usr = UserT2.FromUsername(context, this.Username);
				int userId = User.GetUserId(context, this.Username, authType);
				
                //case user exists and is associated to Eosso Authentication Type, we set his password to the current Session Id
				if (userId != 0) usr.ChangeLdapPassword(HttpContext.Current.Session.SessionID, null, true);

                return usr;
            }catch(Exception e){
                //user does not exist, we create it
            }

            //case user does not exists
            try {
                //email already used, we do not create the new user
                UserT2.FromEmail (context, Email);
                HttpContext.Current.Response.Redirect(context.GetConfigValue("t2portal-emailAlreadyUsedEndpoint"), true);
            } catch (Exception){}

            //create user
            context.AccessLevel = EntityAccessLevel.Administrator;
            usr = (UserT2)User.GetOrCreate(context, Username, authType);
            usr.Email = Email;
            usr.Store();

            usr.LinkToAuthenticationProvider (authType, Username);
            try {
                usr.CreateGithubProfile();
                usr.CreateLdapAccount(HttpContext.Current.Session.SessionID);//we use the sessionId as pwd
				usr.CreateLdapDomain();
                usr.CreateCatalogueIndex();
                usr.CreateRepository();
            }catch(Exception e){
                context.LogError(this, e.Message);
            }
			
			return usr;
        } 

        public override void EndExternalSession(IfyWebContext context, HttpRequest request, HttpResponse response) {
            DBCookie.DeleteDBCookies(context, HttpContext.Current.Session.SessionID);
            response.Headers[HttpHeaders.Location] = context.BaseUrl;
        }

    }
}


