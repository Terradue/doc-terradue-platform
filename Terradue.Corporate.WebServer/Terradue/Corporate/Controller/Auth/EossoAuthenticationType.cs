using System;
using System.Web;
using ServiceStack.Common.Web;
using Terradue.Ldap;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {

    /// <summary>
    /// Authentication open identifier.
    /// </summary>
    public class EossoAuthenticationType : AuthenticationType {

        /// <summary>
        /// The client.
        /// </summary>
        private Connect2IdClient clientSSO;

        private string UserInfoEndpoint;
        private string Username, Email;
        private string Callback;

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
        /// In a derived class, checks whether an session corresponding to the current web server session exists on the
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
        public EossoAuthenticationType(IfyContext context) : base(context) {
            clientSSO = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
        }

        /// <summary>
        /// Sets the SSO Client.
        /// </summary>
        /// <param name="c">Client.</param>
        public void SetCLientSSO (Connect2IdClient c)
        {
            this.clientSSO = c;
        }

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
            try{
                //case user exists
                usr = UserT2.FromUsername(context, this.Username);
                usr.ChangeLdapPassword(HttpContext.Current.Session.SessionID, null, true);
                return usr;
            }catch(Exception e){
                
            }

            //case user does not exists
            AuthenticationType authType = IfyWebContext.GetAuthenticationType(typeof(EossoAuthenticationType));

            try {
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


