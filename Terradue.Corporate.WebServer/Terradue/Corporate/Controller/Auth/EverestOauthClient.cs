using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ServiceStack.Text;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {
    public class EverestOauthClient{

        public string RedirectUri { get; set; }
        public string AuthEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scopes { get; set; }
        public string Callback { get; set; }

        public EverestOauthClient () { }

        public EverestOauthClient (IfyContext context){
            AuthEndpoint = context.GetConfigValue ("everest-authEndpoint");
            ClientId = context.GetConfigValue ("everest-clientId");
            ClientSecret = context.GetConfigValue ("everest-clientSecret");
            TokenEndpoint = context.GetConfigValue ("everest-tokenEndpoint");
            UserInfoEndpoint = context.GetConfigValue ("everest-userInfoEndpoint");
            Callback = context.GetConfigValue ("everest-callback");
            Scopes = context.GetConfigValue ("everest-scopes");
        }

        #region TOKEN

        public string EVEREST_TOKEN_ACCESS { 
            get { 
                return GetCookie (COOKIE_TOKEN_ACCESS);
            }
        }

        public string EVEREST_TOKEN_REFRESH {
            get {
                return GetCookie (COOKIE_TOKEN_REFRESH);
            }
        }

        #endregion

        #region COOKIE

        private const string COOKIE_BASENAME = "t2-sso";
        public const string COOKIE_TOKEN_ACCESS = "external_token_access";
        public const string COOKIE_TOKEN_REFRESH = "external_token_refresh";

        private string GetCookieName (string name) { 
            return string.Format ("{0}_{1}", COOKIE_BASENAME, name);
        }

        /// <summary>
        /// Gets the cookie.
        /// </summary>
        /// <returns>The cookie.</returns>
        /// <param name="name">Name.</param>
        public string GetCookie (string name)
        {
            var cookieName = GetCookieName (name);
            if (HttpContext.Current.Request.Cookies [cookieName] != null)
                return HttpContext.Current.Request.Cookies [cookieName].Value;
            else
                return null;
        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public void SetCookie (string name, string value)
        {
            var cookieName = GetCookieName (name);
            HttpCookie cookie = HttpContext.Current.Request.Cookies [cookieName] ?? new HttpCookie (cookieName);
            cookie.Value = value;
            HttpContext.Current.Response.Cookies.Set (cookie);
        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        /// <param name="expires">Expires.</param>
        public void SetCookie (string name, string value, double expires)
        {
            var cookieName = GetCookieName (name);
            HttpCookie cookie = HttpContext.Current.Request.Cookies [cookieName] ?? new HttpCookie (cookieName);
            cookie.Value = value;
            cookie.Expires = DateTime.UtcNow.AddSeconds (expires);
            HttpContext.Current.Request.Cookies.Set (cookie);
            HttpContext.Current.Response.Cookies.Set (cookie);
        }

        /// <summary>
        /// Revokes the cookie.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RevokeCookie (string name)
        {
            var cookieName = GetCookieName (name);
            HttpCookie cookie = HttpContext.Current.Request.Cookies [cookieName];
            if (cookie != null) {
                cookie.Expires = DateTime.Now.AddDays (-1d);
                HttpContext.Current.Request.Cookies.Set (cookie);
                HttpContext.Current.Response.Cookies.Set (cookie);
            }
        }

        /// <summary>
        /// Revokes all cookies.
        /// </summary>
        public void RevokeAllCookies ()
        {
            RevokeCookie (COOKIE_TOKEN_ACCESS);
            RevokeCookie (COOKIE_TOKEN_REFRESH);
        }

        #endregion

        /// <summary>
        /// Gets the authorization URL.
        /// </summary>
        /// <returns>The authorization URL.</returns>
        public string GetAuthorizationUrl (){
            if (string.IsNullOrEmpty (AuthEndpoint)) throw new Exception ("Invalid Authorization endpoint");

            var scope = Scopes.Replace (",", "%20");
            var redirect_uri = HttpUtility.UrlEncode (Callback);
            var query = string.Format ("response_type={0}&scope={1}&client_id={2}&state={3}&redirect_uri={4}&nonce={5}",
                                          "code", scope, ClientId, Guid.NewGuid ().ToString (), redirect_uri, Guid.NewGuid ().ToString ());

            string url = string.Format ("{0}?{1}",AuthEndpoint,query);

            return url;
        }

        /// <summary>
        /// Accesses the token.
        /// </summary>
        /// <param name="code">Code.</param>
        public void AccessToken (string code) { 
            string url = string.Format("{0}?grant_type=authorization_code&redirect_uri={1}&code={2}", 
                                       TokenEndpoint,
                                       Callback,
                                       code
                                      );
            HttpWebRequest everRequest = (HttpWebRequest)WebRequest.Create (url);
            everRequest.Method = "POST";
            everRequest.ContentType = "application/x-www-form-urlencoded";
            everRequest.Proxy = null;
            everRequest.Headers.Add (HttpRequestHeader.Authorization, "Basic " + GetBasicAuthenticationSecret ());

            try {
                using (var httpResponse = (HttpWebResponse)everRequest.GetResponse ()) {
                    using (var streamReader = new StreamReader (httpResponse.GetResponseStream ())) {
                        var result = streamReader.ReadToEnd ();
                        var response = JsonSerializer.DeserializeFromString<OauthTokenResponse> (result);
                        SetCookie(COOKIE_TOKEN_ACCESS, response.access_token, response.expires_in);
                        SetCookie (COOKIE_TOKEN_REFRESH, response.refresh_token);
                    }
                }
            } catch (Exception) {
                RevokeCookie (COOKIE_TOKEN_ACCESS);
                RevokeCookie (COOKIE_TOKEN_REFRESH);
                throw;
            }
        }

        public void RefreshToken () {
            var scope = Scopes.Replace (",", "%20");
            string url = string.Format ("{0}?client_id={1}&client_secret={2}&grant_type=refresh_token&refresh_token={3}&scope={4}",
                                        TokenEndpoint,
                                        ClientId,
                                        ClientSecret,
                                        EVEREST_TOKEN_REFRESH,
                                        scope
                                      );
            HttpWebRequest everRequest = (HttpWebRequest)WebRequest.Create (url);
            everRequest.Method = "POST";
            everRequest.ContentType = "application/x-www-form-urlencoded";
            everRequest.Proxy = null;

            try {
                using (var httpResponse = (HttpWebResponse)everRequest.GetResponse ()) {
                    using (var streamReader = new StreamReader (httpResponse.GetResponseStream ())) {
                        var result = streamReader.ReadToEnd ();
                        var response = JsonSerializer.DeserializeFromString<OauthTokenResponse> (result);
                        SetCookie (COOKIE_TOKEN_ACCESS, response.access_token, response.expires_in);
                        SetCookie (COOKIE_TOKEN_REFRESH, response.refresh_token);
                    }
                }
            } catch (Exception e) {
                RevokeCookie (COOKIE_TOKEN_ACCESS);
                RevokeCookie (COOKIE_TOKEN_REFRESH);
                throw;
            }
        }

        public void RevokeToken () {
            RevokeCookie (COOKIE_TOKEN_ACCESS);
            RevokeCookie (COOKIE_TOKEN_REFRESH);
        }

        public OauthUserInfoResponse GetUserInfo () {
            OauthUserInfoResponse user;
            string url = string.Format ("{0}", UserInfoEndpoint);
            HttpWebRequest everRequest = (HttpWebRequest)WebRequest.Create (url);
            everRequest.Method = "GET";
            everRequest.ContentType = "application/json";
            everRequest.Proxy = null;
            everRequest.Headers.Add (HttpRequestHeader.Authorization, "Bearer " + EVEREST_TOKEN_ACCESS);

            using (var httpResponse = (HttpWebResponse)everRequest.GetResponse ()) {
                using (var streamReader = new StreamReader (httpResponse.GetResponseStream ())) {
                    var result = streamReader.ReadToEnd ();
                    user = JsonSerializer.DeserializeFromString<OauthUserInfoResponse> (result);
                }
            }
            return user;
        }

        private string GetBasicAuthenticationSecret ()
        {
            return Convert.ToBase64String (Encoding.Default.GetBytes (this.ClientId + ":" + this.ClientSecret));
        }

    }
}
