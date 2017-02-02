using System;
using System.Web;
using ServiceStack.Common.Web;
using Terradue.Ldap;
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
        private Connect2IdClient clientSSO;

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
            clientSSO = new Connect2IdClient (context, context.GetConfigValue ("sso-configUrl"));
        }

        /// <summary>
        /// Sets the Everest Client.
        /// </summary>
        /// <param name="c">Client.</param>
        public void SetCLient(EverestOauthClient c){
            this.client = c;
        }

        /// <summary>
        /// Sets the SSO Client.
        /// </summary>
        /// <param name="c">Client.</param>
        public void SetCLientSSO (Connect2IdClient c)
        {
            this.clientSSO = c;
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

            var refreshToken = client.LoadTokenRefresh().Value;
            var accessToken = client.LoadTokenAccess().Value;
            bool tokenRefreshed = false;
            if (!string.IsNullOrEmpty (refreshToken) && string.IsNullOrEmpty (accessToken)) {
                // refresh the token
                try {
                    client.RefreshToken (refreshToken);
                    accessToken = client.LoadTokenRefresh ().Value;
                    refreshToken = client.LoadTokenAccess ().Value;
                    tokenRefreshed = true;
                } catch (Exception) {
                    return null;
                }
            }
            if (!string.IsNullOrEmpty(accessToken)) {
                OauthUserInfoResponse usrInfo;
                try {
                    usrInfo = client.GetUserInfo (accessToken);
                }catch(Exception) {
                    return null;
                }
                if (usrInfo == null) return null;

                bool exists = User.DoesUserExist(context, usrInfo.sub, authType);

                if (!exists) {
                    bool emailUsed = false;
                    try {
                        UserT2.FromEmail (context, usrInfo.email);
                        emailUsed = true;
                    } catch (Exception){}
                    if (emailUsed) HttpContext.Current.Response.Redirect (context.GetConfigValue ("t2portal-emailAlreadyUsedEndpoint"), true);
                    context.AccessLevel = EntityAccessLevel.Administrator;
                }
                usr = (UserT2)User.GetOrCreate(context, usrInfo.sub, authType);

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
                    usr.LinkToAuthenticationProvider (authType, usrInfo.sub);
                    usr.CreateGithubProfile ();
                    usr.CreateLdapAccount (accessToken);//we use the accesstoken as pwd
                } else if (tokenRefreshed){ //in case of Refresh token
                    usr.ChangeLdapPassword (accessToken, null, true);
                }

                //update domain
                var VRC = string.IsNullOrEmpty (usrInfo.VRC) ? "Citizens" : usrInfo.VRC;
                Domain vrcDomain;
                var domainIdentifier = string.Format ("everest-{0}", VRC);
                try {
                    vrcDomain = Domain.FromIdentifier (context, domainIdentifier);
                } catch (Exception e){
                    //domain does not exists, we create it
                    vrcDomain = new Domain (context);
                    vrcDomain.Identifier = domainIdentifier;
                    vrcDomain.Name = domainIdentifier;
                    vrcDomain.Description = string.Format("Domain of Thematic Group {0} for Everest",domainIdentifier);
                    vrcDomain.Store ();
                }
                context.LogDebug (this, string.Format("Everest user '{0}' is part of domain '{1}'", usr.Username, domainIdentifier));
                //check if user has already a role in the domain
                //if not we add it (+on ldap)
                Role roleVRC = Role.FromIdentifier (context, "member");
                if (!roleVRC.IsGrantedTo (usr, vrcDomain)) { 
                    roleVRC.GrantToUser (usr, vrcDomain);
                    context.LogDebug (this, string.Format ("Everest user '{0}' added as {2} for domain '{1}'", usr.Username, domainIdentifier, roleVRC.Identifier));
                    usr.AddToLdapDomain (domainIdentifier + ".reader", domainIdentifier);
                    context.LogDebug (this, string.Format ("Everest user '{0}' added as {2} on LDAP domain '{1}'", usr.Username, domainIdentifier, domainIdentifier + ".reader"));
                }
                return usr;
            } else {
                      
            }

            return null;
        } 

        public override void EndExternalSession(IfyWebContext context, HttpRequest request, HttpResponse response) {
            client.RevokeSessionCookies ();
            response.Headers[HttpHeaders.Location] = client.GetLogoutUrl ();
        }

        public static bool IsUserCitizens (IfyContext context, User user) { 
            var roleMember = Role.FromIdentifier (context, "member");
            var citizensDomain = Domain.FromIdentifier (context, "everest-Citizens");
            var isMember = roleMember.IsGrantedTo (user, citizensDomain);
            return isMember;
        }

    }
}


