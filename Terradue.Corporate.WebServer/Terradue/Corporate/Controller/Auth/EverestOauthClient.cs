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
        private IfyContext Context; 

        public EverestOauthClient (IfyContext context){
            Context = context;
            AuthEndpoint = context.GetConfigValue ("everest-authEndpoint");
            ClientId = context.GetConfigValue ("everest-clientId");
            ClientSecret = context.GetConfigValue ("everest-clientSecret");
            TokenEndpoint = context.GetConfigValue ("everest-tokenEndpoint");
            UserInfoEndpoint = context.GetConfigValue ("everest-userInfoEndpoint");
            Callback = context.GetConfigValue ("everest-callback");
            Scopes = context.GetConfigValue ("everest-scopes");
        }

        #region TOKEN

        /// <summary>
        /// Loads the token access.
        /// </summary>
        /// <returns>The token access.</returns>
        public DBCookie LoadTokenAccess ()
        {
            return DBCookie.FromSessionAndIdentifier (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS);
        }

        /// <summary>
        /// Stores the token access.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="expire">Expire.</param>
        public void StoreTokenAccess (string value, long expire)
        {
            DBCookie.StoreDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS, value, DateTime.UtcNow.AddSeconds (expire));
        }

        /// <summary>
        /// Deletes the token access.
        /// </summary>
        public void DeleteTokenAccess ()
        {
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS);
        }

        /// <summary>
        /// Loads the token refresh.
        /// </summary>
        /// <returns>The token refresh.</returns>
        public DBCookie LoadTokenRefresh ()
        {
            return DBCookie.FromSessionAndIdentifier (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH);
        }

        /// <summary>
        /// Stores the token refresh.
        /// </summary>
        /// <param name="value">Value.</param>
        public void StoreTokenRefresh (string value)
        {
            DBCookie.StoreDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH, value, DateTime.UtcNow.AddDays (1));
        }

        /// <summary>
        /// Deletes the token refresh.
        /// </summary>
        public void DeleteTokenRefresh ()
        {
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH);
        }

        public void RevokeAllCookies ()
        {
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS);
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH);
        }
        #endregion

        #region COOKIE

                public const string COOKIE_TOKEN_ACCESS = "EVEREST_token_access";
                public const string COOKIE_TOKEN_REFRESH = "EVEREST_token_refresh";

        //        private string GetCookieName (string name) { 
        //            return string.Format ("{0}_{1}", COOKIE_BASENAME, name);
        //        }

        //        /// <summary>
        //        /// Gets the cookie.
        //        /// </summary>
        //        /// <returns>The cookie.</returns>
        //        /// <param name="name">Name.</param>
        //        public string GetCookie (string name)
        //        {
        //            var cookieName = GetCookieName (name);
        //            if (HttpContext.Current.Request.Cookies [cookieName] != null)
        //                return HttpContext.Current.Request.Cookies [cookieName].Value;
        //            else
        //                return null;
        //        }

        //        private HttpCookie CreateCookie (string name, string value)
        //        {
        //            var cookieName = GetCookieName (name);
        //            HttpCookie cookie = HttpContext.Current.Request.Cookies [cookieName] ?? new HttpCookie (cookieName);
        //            cookie.Value = value;
        //#if DEBUG
        //            cookie.Secure = false;
        //#else
        //            cookie.Secure = true;
        //#endif
        //            cookie.HttpOnly = true;
        //            cookie.Domain = HttpContext.Current.Request.Url.Authority;
        //            cookie.Path = "/t2api/";
        //            return cookie;
        //        }

        //        /// <summary>
        //        /// Sets the cookie.
        //        /// </summary>
        //        /// <param name="name">Name.</param>
        //        /// <param name="value">Value.</param>
        //        public void SetCookie (string name, string value)
        //        {
        //            var cookie = CreateCookie (name, value);
        //            HttpContext.Current.Response.Cookies.Set (cookie);
        //        }

        //        /// <summary>
        //        /// Sets the cookie.
        //        /// </summary>
        //        /// <param name="name">Name.</param>
        //        /// <param name="value">Value.</param>
        //        /// <param name="expires">Expires.</param>
        //        public void SetCookie (string name, string value, double expires)
        //        {
        //            var cookie = CreateCookie (name, value);
        //            cookie.Expires = DateTime.UtcNow.AddSeconds (expires);
        //            HttpContext.Current.Response.Cookies.Set (cookie);
        //        }

        //        /// <summary>
        //        /// Revokes the cookie.
        //        /// </summary>
        //        /// <param name="name">Name.</param>
        //        public void RevokeCookie (string name)
        //        {
        //            var cookieName = GetCookieName (name);
        //            HttpContext.Current.Response.Cookies.Remove (cookieName);
        //            HttpContext.Current.Request.Cookies.Remove (cookieName);
        //            //HttpCookie cookie = HttpContext.Current.Request.Cookies [cookieName];
        //            //if (cookie != null) {
        //            //    cookie.Expires = DateTime.Now.AddDays (-10);
        //            //    cookie.Value = null;
        //            //    HttpContext.Current.Response.Cookies.Set (cookie);
        //            //}
        //        }

        //        /// <summary>
        //        /// Revokes all cookies.
        //        /// </summary>
        //        public void RevokeAllCookies ()
        //        {
        //            RevokeCookie (COOKIE_TOKEN_ACCESS);
        //            RevokeCookie (COOKIE_TOKEN_REFRESH);
        //        }

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

                        StoreTokenAccess (response.access_token, response.expires_in);
                        StoreTokenRefresh (response.refresh_token);
                    }
                }
            } catch (Exception e) {
                DeleteTokenAccess ();
                DeleteTokenRefresh ();
                throw e;
            }
        }

        /// <summary>
        /// Refreshs the token.
        /// </summary>
        /// <param name="token">Token.</param>
        public void RefreshToken (string token) {
            var scope = Scopes.Replace (",", "%20");
            string url = string.Format ("{0}?client_id={1}&client_secret={2}&grant_type=refresh_token&refresh_token={3}&scope={4}",
                                        TokenEndpoint,
                                        ClientId,
                                        ClientSecret,
                                        token,
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

                        StoreTokenAccess (response.access_token, response.expires_in);
                        StoreTokenRefresh (response.refresh_token);

                    }
                }
            } catch (Exception e) {
                DeleteTokenAccess ();
                DeleteTokenRefresh ();
                throw e;
            }
        }

        /// <summary>
        /// Gets the user info.
        /// </summary>
        /// <returns>The user info.</returns>
        /// <param name="token">Token.</param>
        public OauthUserInfoResponse GetUserInfo (string token) {
            OauthUserInfoResponse user;
            string url = string.Format ("{0}", UserInfoEndpoint);
            HttpWebRequest everRequest = (HttpWebRequest)WebRequest.Create (url);
            everRequest.Method = "GET";
            everRequest.ContentType = "application/json";
            everRequest.Proxy = null;
            everRequest.Headers.Add (HttpRequestHeader.Authorization, "Bearer " + token);

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
