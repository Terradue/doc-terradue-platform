using System;
using System.Web;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {

    /// <summary>
    /// Authentication open identifier.
    /// </summary>
    public class EverestAuthenticationType : AuthenticationType {

        /// <summary>
        /// The client.
        /// </summary>
        private EverestOauthClient client;

        private string UserInfoEndpoint;
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
        public EverestAuthenticationType(IfyContext context) : base(context) {
            client = new EverestOauthClient (context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Authentication.OAuth.OAuth2AuthenticationType"/> class.
        /// </summary>
        /// <param name="c">C.</param>
        public void SetCLient(EverestOauthClient c){
            this.client = c;
        }

        /// <summary>
        /// Gets the user profile.
        /// </summary>
        /// <returns>The user profile.</returns>
        /// <param name="context">Context.</param>
        /// <param name="request">Request.</param>
        public override User GetUserProfile(IfyWebContext context, HttpRequest request = null, bool strict = false){
            UserT2 usr;
            AuthenticationType authType = IfyWebContext.GetAuthenticationType(typeof(EverestAuthenticationType));

            bool tokenRefreshed = false;
            if (!string.IsNullOrEmpty(client.GetRefreshToken()) && string.IsNullOrEmpty (client.GetAccessToken ())) {
                client.RefreshToken ();
                tokenRefreshed = true;
            }
            if (!string.IsNullOrEmpty(client.GetAccessToken ())) {
                OauthUserInfoResponse usrInfo;
                try {
                    usrInfo = client.GetUserInfo ();
                }catch(Exception) {
                    return null;
                }
                if (usrInfo == null) return null;

                bool exists = User.DoesUserExist(context, usrInfo.sub, authType);
                usr = (UserT2)User.GetOrCreate(context, usrInfo.sub);

                if (usr.AccountStatus == AccountStatusType.Disabled) usr.AccountStatus = AccountStatusType.Enabled;

                //update user infos
                if (!string.IsNullOrEmpty(usrInfo.given_name))
                    usr.FirstName = usrInfo.given_name;
                if (!string.IsNullOrEmpty(usrInfo.family_name))
                    usr.LastName = usrInfo.family_name;
                if (!string.IsNullOrEmpty(usrInfo.email))
                    usr.Email = usrInfo.email;
                if (!string.IsNullOrEmpty(usrInfo.zoneinfo))
                    usr.TimeZone = usrInfo.zoneinfo;
                if (!string.IsNullOrEmpty(usrInfo.locale))
                    usr.Language = usrInfo.locale;

                usr.Store();
                if (!exists) {
                    usr.LinkToAuthenticationProvider (authType, usr.Username);
                    usr.CreateGithubProfile ();
                    usr.CreateLdapAccount (client.GetAccessToken ());
                } else if (tokenRefreshed){ //in case of Refresh token
                    usr.ChangeLdapPassword (client.GetAccessToken (), null, true);
                }

                return usr;
            } else {
                      
            }

            return null;
        } 

        public override void EndExternalSession(IfyWebContext context, HttpRequest request, HttpResponse response) {
            
            client.RevokeToken();

            HttpCookie cookie = new HttpCookie("t2-sso-externalTokenAccess");
            cookie.Expires = DateTime.Now.AddDays(-1d);
            HttpContext.Current.Response.Cookies.Add(cookie);

            cookie = new HttpCookie ("t2-sso-externalTokenRefresh");
            cookie.Expires = DateTime.Now.AddDays (-1d);
            HttpContext.Current.Response.Cookies.Add (cookie);

        }

    }
}

