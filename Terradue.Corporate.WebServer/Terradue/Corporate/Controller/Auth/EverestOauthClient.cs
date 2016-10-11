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

        #region Cookies

        public string GetAccessToken ()
        {
            if (HttpContext.Current.Request.Cookies ["t2-sso"] != null)
                return HttpContext.Current.Request.Cookies ["t2-sso"] ["everest-token_access"];
            else return null;
        }

        public void SetAccessToken (string value, double expires) {
            HttpCookie cookie = HttpContext.Current.Request.Cookies ["t2-sso"] ?? new HttpCookie ("t2-sso");
            cookie ["everest-token_access"] = value;
            cookie.Expires = DateTime.UtcNow.AddSeconds (expires);
            HttpContext.Current.Response.Cookies.Set (cookie);
        }

        public string GetRefreshToken ()
        {
            if (HttpContext.Current.Request.Cookies ["t2-sso"] != null)
                return HttpContext.Current.Request.Cookies ["t2-sso"] ["everest-token_refresh"];
            else return null;
        }

        public void SetRefreshToken (string value, double expires)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies ["t2-sso"] ?? new HttpCookie ("t2-sso");
            cookie ["everest-token_refresh"] = value;
            cookie.Expires = DateTime.UtcNow.AddSeconds (expires);
            HttpContext.Current.Response.Cookies.Set (cookie);
        }

        public double GetTokenExpiresSecond ()
        {
            if (HttpContext.Current.Request.Cookies ["t2-sso"] != null)
                return Math.Max ((HttpContext.Current.Request.Cookies ["t2-sso"].Expires - DateTime.UtcNow).TotalSeconds, 0);
            else
                return 0;
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
                        SetAccessToken (response.access_token, response.expires_in);
                        SetRefreshToken (response.refresh_token, response.expires_in);
                    }
                }
            } catch (Exception) {
                SetAccessToken (null, 0);
                SetRefreshToken (null, 0);
                throw;
            }
        }

        public void RefreshToken () {
            var scope = Scopes.Replace (",", "%20");
            string url = string.Format ("{0}?client_id={1}&client_secret={2}&grant_type=refresh_token&refresh_token={3}&scope={4}",
                                        TokenEndpoint,
                                        ClientId,
                                        ClientSecret,
                                        GetRefreshToken(),
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
                        SetAccessToken (response.access_token, response.expires_in);
                        SetRefreshToken (response.refresh_token, response.expires_in);
                    }
                }
            } catch (Exception e) {
                SetAccessToken (null, 0);
                SetRefreshToken (null, 0);
                throw;
            }
        }

        public void RevokeToken () {
            SetAccessToken (null, 0);
            SetRefreshToken (null, 0);
        }

        public OauthUserInfoResponse GetUserInfo () {
            OauthUserInfoResponse user;
            string url = string.Format ("{0}", UserInfoEndpoint);
            HttpWebRequest everRequest = (HttpWebRequest)WebRequest.Create (url);
            everRequest.Method = "GET";
            everRequest.ContentType = "application/json";
            everRequest.Proxy = null;
            everRequest.Headers.Add (HttpRequestHeader.Authorization, "Bearer " + GetAccessToken());

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
