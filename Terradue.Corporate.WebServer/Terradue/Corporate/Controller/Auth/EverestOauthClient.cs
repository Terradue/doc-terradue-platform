using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ServiceStack.Text;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {
    public class EverestOauthClient : OpenIdOauthClient {

        public EverestOauthClient(IfyContext context) : base(context) {
            AuthEndpoint = context.GetConfigValue("everest-authEndpoint");
            ClientName = context.GetConfigValue("everest-clientName");
            ClientId = context.GetConfigValue("everest-clientId");
            ClientSecret = context.GetConfigValue("everest-clientSecret");
            TokenEndpoint = context.GetConfigValue("everest-tokenEndpoint");
            LogoutEndpoint = context.GetConfigValue("everest-logoutEndpoint");
            UserInfoEndpoint = context.GetConfigValue("everest-userInfoEndpoint");
            Callback = context.GetConfigValue("everest-callback");
            Scopes = context.GetConfigValue("everest-scopes");

            COOKIE_TOKEN_ACCESS = "EVEREST_token_access";
            COOKIE_TOKEN_REFRESH = "EVEREST_token_refresh";
            COOKIE_TOKEN_ID = "EVEREST_token_id";

	}

        public override string GetLogoutUrl() {
			return string.Format("{0}?commonAuthLogout=true&type=oidc&commonAuthCallerPath={1}&relyingParty={2}",
								  LogoutEndpoint,
								  HttpUtility.UrlEncode(Context.BaseUrl),
								  ClientName);
		}
    }
}
