using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ServiceStack.Text;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {
	public class OpenIdOauthClient {

		public string RedirectUri { get; set; }
		public string AuthEndpoint { get; set; }
		public string TokenEndpoint { get; set; }
		public string UserInfoEndpoint { get; set; }
		public string LogoutEndpoint { get; set; }
		public string ClientName { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string Scopes { get; set; }
		public string Callback { get; set; }

		public string COOKIE_TOKEN_ACCESS = "OPENID_token_access";
		public string COOKIE_TOKEN_REFRESH = "OPENID_token_refresh";
        public string COOKIE_TOKEN_ID = "OPENID_token_id";

		protected IfyContext Context;

		public OpenIdOauthClient(IfyContext context) {
			Context = context;

			ServicePointManager.ServerCertificateValidationCallback = delegate (
				Object obj, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain,
				System.Net.Security.SslPolicyErrors errors) { return (true); };
		}

		#region TOKEN

		/// <summary>
		/// Loads the token access.
		/// </summary>
		/// <returns>The token access.</returns>
		public DBCookie LoadTokenAccess() {
			if (HttpContext.Current.Session == null) return new DBCookie(Context);
			return DBCookie.FromSessionAndIdentifier(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_ACCESS);
		}

		/// <summary>
		/// Stores the token access.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="expire">Expire.</param>
		public void StoreTokenAccess(string value, long expire) {
			if (HttpContext.Current.Session == null) return;
            if (string.IsNullOrEmpty(value)) return;
			DBCookie.StoreDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_ACCESS, value, DateTime.UtcNow.AddSeconds(expire));
		}

		/// <summary>
		/// Deletes the token access.
		/// </summary>
		public void DeleteTokenAccess() {
			if (HttpContext.Current.Session == null) return;
            try {
                DBCookie.DeleteDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_ACCESS);
            }catch(Exception){}
		}

		/// <summary>
		/// Loads the token refresh.
		/// </summary>
		/// <returns>The token refresh.</returns>
		public DBCookie LoadTokenRefresh() {
			if (HttpContext.Current.Session == null) return new DBCookie(Context);
			return DBCookie.FromSessionAndIdentifier(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_REFRESH);
		}

		/// <summary>
		/// Stores the token refresh.
		/// </summary>
		/// <param name="value">Value.</param>
		public void StoreTokenRefresh(string value) {
			if (HttpContext.Current.Session == null) return;
            if (string.IsNullOrEmpty(value)) return;
			DBCookie.StoreDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_REFRESH, value, DateTime.UtcNow.AddDays(1));
		}

		/// <summary>
		/// Deletes the token refresh.
		/// </summary>
		public void DeleteTokenRefresh() {
			if (HttpContext.Current.Session == null) return;
            try{
			    DBCookie.DeleteDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_REFRESH);
            } catch (Exception) { }
		}

		/// <summary>
		/// Loads the token id.
		/// </summary>
		/// <returns>The token id.</returns>
		public DBCookie LoadTokenId() {
			if (HttpContext.Current.Session == null) return new DBCookie(Context);
			return DBCookie.FromSessionAndIdentifier(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_ID);
		}

		/// <summary>
		/// Stores the token id.
		/// </summary>
		/// <param name="value">Value.</param>
		public void StoreTokenId(string value) {
			if (HttpContext.Current.Session == null) return;
			if (string.IsNullOrEmpty(value)) return;
			DBCookie.StoreDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_ID, value, DateTime.UtcNow.AddDays(1));
		}

		/// <summary>
		/// Deletes the token id.
		/// </summary>
		public void DeleteTokenId() {
			if (HttpContext.Current.Session == null) return;
			try {
				DBCookie.DeleteDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_ID);
			} catch (Exception) { }
		}

		public void RevokeAllCookies() {
			if (HttpContext.Current.Session == null) return;
            try{
    			DBCookie.DeleteDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_ACCESS);
	    		DBCookie.DeleteDBCookie(Context, HttpContext.Current.Session.SessionID, COOKIE_TOKEN_REFRESH);
            } catch (Exception) { }
		}

		public void RevokeSessionCookies() {
			if (HttpContext.Current.Session == null) return;
			DBCookie.DeleteDBCookies(Context, HttpContext.Current.Session.SessionID);
		}
		#endregion

		/// <summary>
		/// Gets the authorization URL.
		/// </summary>
		/// <returns>The authorization URL.</returns>
		public string GetAuthorizationUrl() {
			if (string.IsNullOrEmpty(AuthEndpoint)) throw new Exception("Invalid Authorization endpoint");

			var scope = Scopes.Replace(",", "%20");
			var redirect_uri = HttpUtility.UrlEncode(Callback);
			var query = string.Format("response_type={0}&scope={1}&client_id={2}&state={3}&redirect_uri={4}&nonce={5}",
										  "code", scope, ClientId, Guid.NewGuid().ToString(), redirect_uri, Guid.NewGuid().ToString());

			string url = string.Format("{0}?{1}", AuthEndpoint, query);

			return url;
		}

		/// <summary>
		/// Accesses the token.
		/// </summary>
		/// <param name="code">Code.</param>
		public void AccessToken(string code) {

            var scope = Scopes.Replace(",", "%20");
			string url = string.Format("{0}",TokenEndpoint);
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.Proxy = null;
			webRequest.Headers.Add(HttpRequestHeader.Authorization, "Basic " + GetBasicAuthenticationSecret());

            var dataStr = string.Format("grant_type=authorization_code&redirect_uri={0}&code={1}&scope={2}",HttpUtility.UrlEncode(Callback),code,scope);
			byte[] data = System.Text.Encoding.UTF8.GetBytes(dataStr);

            webRequest.ContentLength = data.Length;

			using (var requestStream = webRequest.GetRequestStream()) {
				requestStream.Write(data, 0, data.Length);
				requestStream.Close();
				try {
					using (var httpResponse = (HttpWebResponse)webRequest.GetResponse()) {
						using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
							var result = streamReader.ReadToEnd();
							var response = JsonSerializer.DeserializeFromString<OauthTokenResponse>(result);
							StoreTokenAccess(response.access_token, response.expires_in);
							StoreTokenRefresh(response.refresh_token);
                            StoreTokenId(response.id_token);
						}
					}
				} catch (Exception e) {
					DeleteTokenAccess();
					DeleteTokenRefresh();
                    DeleteTokenId();
					throw e;
				}
			}
		}

		/// <summary>
		/// Refreshs the token.
		/// </summary>
		/// <param name="token">Token.</param>
		public void RefreshToken(string token) {

			var scope = Scopes.Replace(",", "%20");
			string url = string.Format("{0}",TokenEndpoint);
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.Proxy = null;

			var dataStr = string.Format("client_id={0}&client_secret={1}&grant_type=refresh_token&refresh_token={2}&scope={3}", ClientId, ClientSecret, token, scope);
			byte[] data = System.Text.Encoding.UTF8.GetBytes(dataStr);

            webRequest.ContentLength = data.Length;

            using (var requestStream = webRequest.GetRequestStream()) {
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                try {
                    using (var httpResponse = (HttpWebResponse)webRequest.GetResponse()) {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                            var result = streamReader.ReadToEnd();
                            var response = JsonSerializer.DeserializeFromString<OauthTokenResponse>(result);

                            StoreTokenAccess(response.access_token, response.expires_in);
                            StoreTokenRefresh(response.refresh_token);

                        }
                    }
                } catch (Exception e) {
                    DeleteTokenAccess();
                    DeleteTokenRefresh();
                    DeleteTokenId();
                    throw e;
                }
            }
		}

		/// <summary>
		/// Gets the user info.
		/// </summary>
		/// <returns>The user info.</returns>
		/// <param name="token">Token.</param>
		public OauthUserInfoResponse GetUserInfo(string token) {

			OauthUserInfoResponse user;
			string url = string.Format("{0}", UserInfoEndpoint);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Method = "GET";
			webRequest.ContentType = "application/json";
			webRequest.Proxy = null;
			webRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);

			using (var httpResponse = (HttpWebResponse)webRequest.GetResponse()) {
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
					var result = streamReader.ReadToEnd();
					user = JsonSerializer.DeserializeFromString<OauthUserInfoResponse>(result);
				}
			}
			return user;
		}

		private string GetBasicAuthenticationSecret() {
			return Convert.ToBase64String(Encoding.Default.GetBytes(this.ClientId + ":" + this.ClientSecret));
		}

		public virtual string GetLogoutUrl() {
			return string.Format("{0}",LogoutEndpoint);
		}

	}
}
