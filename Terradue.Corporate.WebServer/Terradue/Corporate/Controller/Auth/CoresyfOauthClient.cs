using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ServiceStack.Text;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {
    public class CoresyfOauthClient : OpenIdOauthClient {

		public CoresyfOauthClient(IfyContext context) : base(context) {
			AuthEndpoint = context.GetConfigValue("coresyf-authEndpoint");
			ClientName = context.GetConfigValue("coresyf-clientName");
			ClientId = context.GetConfigValue("coresyf-clientId");
			ClientSecret = context.GetConfigValue("coresyf-clientSecret");
			TokenEndpoint = context.GetConfigValue("coresyf-tokenEndpoint");
			LogoutEndpoint = context.GetConfigValue("coresyf-logoutEndpoint");
			UserInfoEndpoint = context.GetConfigValue("coresyf-userInfoEndpoint");
			Callback = context.GetConfigValue("coresyf-callback");
			Scopes = context.GetConfigValue("coresyf-scopes");

			COOKIE_TOKEN_ACCESS = "CORESYF_token_access";
			COOKIE_TOKEN_REFRESH = "CORESYF_token_refresh";
            COOKIE_TOKEN_ID = "CORESYF_token_id";
		}

        /// <summary>
        /// Gets the logout URL.
        /// </summary>
        /// <returns>The logout URL.</returns>
		public override string GetLogoutUrl() {
            return string.Format("{0}?id_token_hint={1}&post_logout_redirect_uri={2}",LogoutEndpoint, LoadTokenId().Value, Context.BaseUrl);
		}

		/// <summary>
		/// Deletes the session.
		/// </summary>
		public void DeleteSession() {

			var url = GetLogoutUrl();
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Method = "GET";
			webRequest.Proxy = null;

			using (var httpResponse = (HttpWebResponse)webRequest.GetResponse()) {
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
					var result = streamReader.ReadToEnd();
				}
			}
		}
	}
}
