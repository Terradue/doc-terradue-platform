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
        public string LogoutEndpoint { get; set; }
        public string ClientName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scopes { get; set; }
        public string Callback { get; set; }
        private IfyContext Context; 

        public EverestOauthClient (IfyContext context){
            Context = context;
            AuthEndpoint = context.GetConfigValue ("everest-authEndpoint");
            ClientName = context.GetConfigValue ("everest-clientName");
            ClientId = context.GetConfigValue ("everest-clientId");
            ClientSecret = context.GetConfigValue ("everest-clientSecret");
            TokenEndpoint = context.GetConfigValue ("everest-tokenEndpoint");
            LogoutEndpoint = context.GetConfigValue ("everest-logoutEndpoint");
            UserInfoEndpoint = context.GetConfigValue ("everest-userInfoEndpoint");
            Callback = context.GetConfigValue ("everest-callback");
            Scopes = context.GetConfigValue ("everest-scopes");

            ServicePointManager.ServerCertificateValidationCallback = delegate (
                Object obj, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors errors) { return (true); };
        }

        #region TOKEN

        /// <summary>
        /// Loads the token access.
        /// </summary>
        /// <returns>The token access.</returns>
        public DBCookie LoadTokenAccess ()
        {
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return new DBCookie (Context);
            return DBCookie.FromSessionAndIdentifier (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS);
        }

        /// <summary>
        /// Stores the token access.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="expire">Expire.</param>
        public void StoreTokenAccess (string value, long expire)
        {
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return;
            DBCookie.StoreDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS, value, DateTime.UtcNow.AddSeconds (expire));
        }

        /// <summary>
        /// Deletes the token access.
        /// </summary>
        public void DeleteTokenAccess ()
        {
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return;
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS);
        }

        /// <summary>
        /// Loads the token refresh.
        /// </summary>
        /// <returns>The token refresh.</returns>
        public DBCookie LoadTokenRefresh ()
        {
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return new DBCookie (Context);
            return DBCookie.FromSessionAndIdentifier (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH);
        }

        /// <summary>
        /// Stores the token refresh.
        /// </summary>
        /// <param name="value">Value.</param>
        public void StoreTokenRefresh (string value)
        {
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return;
            DBCookie.StoreDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH, value, DateTime.UtcNow.AddDays (1));
        }

        /// <summary>
        /// Deletes the token refresh.
        /// </summary>
        public void DeleteTokenRefresh ()
        {
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return;
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH);
        }

        public void RevokeAllCookies ()
        {
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return;
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_ACCESS);
            DBCookie.DeleteDBCookie (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value, COOKIE_TOKEN_REFRESH);
        }

        public void RevokeSessionCookies () { 
            if (HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"] == null) return;
            DBCookie.DeleteDBCookies (Context, HttpContext.Current.Request.Cookies ["ASP.NET_SessionId"].Value);
        }
        #endregion

        #region COOKIE

                public const string COOKIE_TOKEN_ACCESS = "EVEREST_token_access";
                public const string COOKIE_TOKEN_REFRESH = "EVEREST_token_refresh";

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

        public string GetLogoutUrl () { 
            return string.Format ("{0}?commonAuthLogout=true&type=oidc&commonAuthCallerPath={1}&relyingParty={2}", 
                                  LogoutEndpoint, 
                                  HttpUtility.UrlEncode(Context.BaseUrl), 
                                  ClientName);
        }

    }
}
