using System;
using System.Web;
using ServiceStack.Common.Web;
using Terradue.Ldap;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {

    /// <summary>
    /// Authentication open identifier.
    /// </summary>
    public class CoresyfAuthenticationType : AuthenticationType {

		/// <summary>
		/// The client.
		/// </summary>
		private CoresyfOauthClient client;
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
		public override bool IsExternalSessionActive(IfyWebContext context, HttpRequest request) {
			return true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Terradue.OAuth.CoresyfAuthenticationType"/> class.
		/// </summary>
		public CoresyfAuthenticationType(IfyContext context) : base(context) {
			client = new CoresyfOauthClient(context);
			clientSSO = new Connect2IdClient(context, context.GetConfigValue("sso-configUrl"));
		}

		/// <summary>
		/// Sets the Openid Client.
		/// </summary>
		/// <param name="c">Client.</param>
		public void SetCLient(CoresyfOauthClient c) {
			this.client = c;
		}

		/// <summary>
		/// Sets the SSO Client.
		/// </summary>
		/// <param name="c">Client.</param>
		public void SetCLientSSO(Connect2IdClient c) {
			this.clientSSO = c;
		}

		/// <summary>
		/// Gets the user profile.
		/// </summary>
		/// <returns>The user profile.</returns>
		/// <param name="context">Context.</param>
		/// <param name="request">Request.</param>
		public override User GetUserProfile(IfyWebContext context, HttpRequest request = null, bool strict = false) {
			UserT2 usr;
			AuthenticationType authType = IfyWebContext.GetAuthenticationType(typeof(CoresyfAuthenticationType));

			var refreshToken = client.LoadTokenRefresh().Value;
			var accessToken = client.LoadTokenAccess().Value;
			bool tokenRefreshed = false;
			if (!string.IsNullOrEmpty(refreshToken) && string.IsNullOrEmpty(accessToken)) {
				// refresh the token
				try {
					client.RefreshToken(refreshToken);
					accessToken = client.LoadTokenRefresh().Value;
					refreshToken = client.LoadTokenAccess().Value;
					tokenRefreshed = true;
				} catch (Exception) {
					return null;
				}
			}
			if (!string.IsNullOrEmpty(accessToken)) {
				OauthUserInfoResponse usrInfo;
				try {
					usrInfo = client.GetUserInfo(accessToken);
				} catch (Exception) {
					return null;
				}
				if (usrInfo == null) return null;

                bool exists = User.DoesUserExist(context, usrInfo.user_name, authType);

				if (!exists) {
					bool emailUsed = false;
					try {
						UserT2.FromEmail(context, usrInfo.email);
						emailUsed = true;
					} catch (Exception) { }
					if (emailUsed) {
						client.RevokeSessionCookies();
						throw new EmailAlreadyUsedException("Email already used, cannot create new user");
					}
					context.AccessLevel = EntityAccessLevel.Administrator;
				}
				usr = (UserT2)User.GetOrCreate(context, usrInfo.user_name, authType);

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
                    usr.LinkToAuthenticationProvider(authType, usrInfo.user_name);
					usr.CreateGithubProfile();
					usr.CreateLdapAccount(accessToken);//we use the accesstoken as pwd
				} else if (tokenRefreshed) { //in case of Refresh token
					usr.ChangeLdapPassword(accessToken, null, true);
				}

				//update domain
				//TODO

				return usr;
			} else {

			}

			return null;
		}

		public override void EndExternalSession(IfyWebContext context, HttpRequest request, HttpResponse response) {

            //TODO: currently the endsession redirect is not working on Coresyf side
            //The correct behaviour would be to redirect on this page instead of calling the DeleteSession
            //When it is working back, uncomment the following line and comment the one after
			//response.Headers[HttpHeaders.Location] = client.GetLogoutUrl();
			client.DeleteSession();

            client.RevokeSessionCookies();
		}
	}
}


