using System;
using ServiceStack.ServiceHost;
using Terradue.Ldap;
using Terradue.Corporate.WebServer.Common;
using Terradue.Portal;
using System.Web;
using Terradue.Corporate.Controller;
using System.Collections.Generic;
using System.Security.Cryptography;
using ServiceStack.Common.Web;
using System.Net;

namespace Terradue.Corporate.WebServer {

    [Route("/oauth/eosso", "GET")]
    public class OauthEoSsoRequest {
        [ApiMember(Name = "return_to", Description = "return_to url", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string return_to { get; set; }
    }

    [Route("/eosso/cb", "GET")]
    public class OauthEossoCallBackRequest {
        [ApiMember(Name = "payload", Description = "oauth payload", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string payload { get; set; }

        [ApiMember(Name = "sig", Description = "oauth sig", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string sig { get; set; }
    }

    [Route("/eosso/user", "GET")]
    public class GetUserFromSSORequest : IReturn<WebUserT2> {
        [ApiMember(Name = "payload", Description = "oauth payload", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string payload { get; set; }

        [ApiMember(Name = "sig", Description = "oauth sig", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string sig { get; set; }
    }

    [Route("/sso/user", "POST")]
    public class PostUserFromSSORequest : WebUserT2 {
    	[ApiMember(Name = "eosso", Description = "eosso name", ParameterType = "query", DataType = "string", IsRequired = true)]
    	public string EoSSO { get; set; }

    	[ApiMember(Name = "token", Description = "token", ParameterType = "query", DataType = "string", IsRequired = true)]
    	public string Token { get; set; }

    	[ApiMember(Name = "originator", Description = "system making the request", ParameterType = "query", DataType = "string", IsRequired = false)]
    	public string Originator { get; set; }

    	[ApiMember(Name = "domain", Description = "plan's domain at user creation", ParameterType = "query", DataType = "string", IsRequired = false)]
    	public string Domain { get; set; }
    }

    public class WebEossoUser {
        [ApiMember(Name = "username", Description = "username", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Username { get; set; }

        [ApiMember(Name = "apikey", Description = "apikey", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string ApiKey { get; set; }
    }

    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    /// <summary>
    /// OAuth service. Used to log into the system
    /// </summary>
    public class OAuthEossoService : ServiceStack.ServiceInterface.Service {
        /// <summary>
        /// Get the specified request. Used to log in user via EOSSO
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="request">Request.</param>
        public object Get(OauthEoSsoRequest request) {
            string redirect;
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                context.LogInfo(this, string.Format("/oauth/eosso GET"));

                //save nonce in session, will be used as security check in callback function
                var nonce = Guid.NewGuid().ToString();
                HttpContext.Current.Session["eosso-nonce"] = nonce;
                var callback = context.BaseUrl + "/eosso/cb";

                //build payload (in base 64) + SIG (used to validate the payload on EOSSO side, using the same secret key)
                var payload = string.Format("nonce={0}&redirect_uri={1}", nonce, callback);
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                byte[] payloadBytes = encoding.GetBytes(payload);
                var sso = System.Convert.ToBase64String(payloadBytes);
                var sig = OAuthUtils.HashHMAC(context.GetConfigValue("sso-eosso-secret"), sso);

                redirect = string.Format("{0}?payload={1}&sig={2}",
                                         context.GetConfigValue("sso-eosso-endpoint"),
                                         sso,
                                         sig);

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }

            return OAuthUtils.DoRedirect(context, redirect, false);
        }

        public object Get(OauthEossoCallBackRequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            var redirect = "";
            UserT2 user = null;
            try {
                context.Open();

                context.LogInfo(this, string.Format("/eosso/cb GET"));

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                //validate the payload using the SIG generated on EOSSO with the same secret key
                var sig = OAuthUtils.HashHMAC(context.GetConfigValue("sso-eosso-secret"), request.payload);
                if (!sig.Equals(request.sig)) throw new Exception("Invalid payload");

                //validate payload 
                var base64Payload = System.Convert.FromBase64String(request.payload);
                var payload = encoding.GetString(base64Payload);
                var querystring = HttpUtility.ParseQueryString(payload);
                var nonce = querystring["nonce"];
                var username = querystring["username"];
                var email = querystring["email"];

                //security check using nonce (should be the same as the one stored in session)
                if (!nonce.Equals(HttpContext.Current.Session["eosso-nonce"])) throw new Exception("Invalid nonce");

                //get user from username/email
                var auth = new EossoAuthenticationType(context);
                auth.SetUserInformation(username, email);
                try {
                    user = (UserT2)auth.GetUserProfile(context);
                }catch(EmailAlreadyUsedException){
                    OAuthUtils.DoRedirect(context, context.GetConfigValue("t2portal-emailAlreadyUsedEndpoint"), false);
				}
                if (user == null) throw new Exception("Error to load user");
                context.LogDebug(this, string.Format("Loaded user '{0}'", user.Username));

                //start user session
                context.StartSession(auth, user);
                context.SetUserInformation(auth, user);

                //Create the session also on SSO
                var clientSSO = new Connect2IdClient(context, context.GetConfigValue("sso-configUrl"));
                clientSSO.SSOAuthEndpoint = context.GetConfigValue("sso-authEndpoint");
                clientSSO.SSOApiClient = context.GetConfigValue("sso-clientId");
                clientSSO.SSOApiSecret = context.GetConfigValue("sso-clientSecret");
                clientSSO.SSOApiToken = context.GetConfigValue("sso-apiAccessToken");
                clientSSO.SSODirectAuthEndpoint = context.GetConfigValue("sso-directAuthEndpoint");

                var defaultscopes = new List<string>(context.GetConfigValue("sso-scopes").Split(",".ToCharArray()));

                var span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

                var directAuthRequest = new OauthDirectAuthzRequest {
                    client_id = clientSSO.SSOApiClient,
                    sub_session = new OauthSubSessionRequest {
                        sub = user.Username,
                        auth_time = (long)span.TotalSeconds,
                        creation_time = (long)span.TotalSeconds
                    },
                    refresh_token = new OauthRefreshToken {
                        issue = true,
                        lifetime = 3600
                    },
                    long_lived = true,
                    scope = defaultscopes
                };

                //create the SID (bypassing user credentials with direct authz
                var directAuthResponse = clientSSO.DirectAuthorization(directAuthRequest);
                var sid = directAuthResponse.sub_sid;
                var accesstoken = directAuthResponse.access_token;
                var refreshtoken = directAuthResponse.refresh_token;
                if (string.IsNullOrEmpty(sid)) throw new Exception("SID received is empty");
                clientSSO.StoreSUBSID(sid);
                if (!string.IsNullOrEmpty(accesstoken)) {
                    clientSSO.StoreTokenAccess(accesstoken, directAuthResponse.expires_in);
                }
                if (!string.IsNullOrEmpty(refreshtoken)) {
                    clientSSO.StoreTokenRefresh(refreshtoken);
                }

                redirect = context.GetConfigValue("t2portal-welcomeEndpoint");

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return OAuthUtils.DoRedirect(context, redirect, false);
        }

        public object Get(GetUserFromSSORequest request) {
            T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
            UserT2 user = null;
            WebEossoUser result = null;
            try {
                context.Open();
                context.LogInfo(this, string.Format("/eosso/user GET"));

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                //validate the payload using the SIG generated on EOSSO with the same secret key
                var sig = OAuthUtils.HashHMAC(context.GetConfigValue("sso-eosso-secret"), request.payload);
                if (!sig.Equals(request.sig)) throw new Exception("Invalid payload");

                //read payload 
                var base64Payload = System.Convert.FromBase64String(request.payload);
                var payload = encoding.GetString(base64Payload);
                var querystring = HttpUtility.ParseQueryString(payload);
                var username = querystring["username"];
                var email = querystring["email"];
                var originator = querystring["originator"];
                var plan = querystring["plan"];

                context.LogDebug(this, string.Format("/sso/user GET username='{0}',email='{1}'", username, email));

                //get user from username/email
                var auth = new EossoAuthenticationType(context);
                auth.SetUserInformation(username, email, originator);
                user = (UserT2)auth.GetUserProfile(context);
                if (user == null) throw new Exception("Error to load user");
                context.LogDebug(this, string.Format("Loaded user '{0}'", user.Username));

                if (!string.IsNullOrEmpty(plan)) {
                    try {
                        var t2plan = new Plan();
                        t2plan.Role = Role.FromIdentifier(context, "plan_" + plan);
                        user.Upgrade(t2plan);
                    } catch (Exception) {
                        context.LogError(this, "Invalid plan parameter");
                    }
                }

                user.LoadApiKey();

				result = new WebEossoUser { 
                    Username = user.Username,
                    ApiKey = user.ApiKey
                };

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message);
                context.Close();
                throw e;
            }
            return result;
        }

        public object Post(PostUserFromSSORequest request) {
        	T2CorporateWebContext context = new T2CorporateWebContext(PagePrivileges.EverybodyView);
        	WebUserT2 result = null;
        	var plan = new Plan();
        	try {
        		context.Open();
        		context.LogInfo(this, string.Format("/sso/user POST eosso='{0}',email='{1}',originator='{2}'", request.EoSSO, request.Email, request.Originator));

        		//check request token
        		if (string.IsNullOrEmpty(request.Token) || !request.Token.Equals(context.GetConfigValue("t2portal-token-usrsso"))) {
        			return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid token parameter"));
        		}
        		//check request username
        		if (string.IsNullOrEmpty(request.Username)) {
        			return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid username parameter"));
        		}
        		//check request password
        		//if (string.IsNullOrEmpty(request.Password)) {
        		//	return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid password parameter"));
        		//} else {
        		//	try {
        		//		UserT2.ValidatePassword(request.Password);
        		//	} catch (Exception e) {
        		//		return new HttpError(HttpStatusCode.BadRequest, e);
        		//	}
        		//}
        		//check request eosso
        		if (string.IsNullOrEmpty(request.EoSSO)) {
        			return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid eosso parameter"));
        		}
        		//check request email
        		if (string.IsNullOrEmpty(request.Email) || !request.Email.Contains("@")) {
        			return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid email parameter"));
        		}
        		//check request plan
        		if (!string.IsNullOrEmpty(request.Plan)) {
        			try {
        				plan.Role = Role.FromIdentifier(context, "plan_" + request.Plan);
        			} catch (Exception) {
        				return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid plan parameter"));
        			}
        		}
        		//check request domain
        		if (!string.IsNullOrEmpty(request.Domain)) {
        			try {
        				plan.Domain = Domain.FromIdentifier(context, request.Domain);
        			} catch (Exception) {
        				return new HttpError(HttpStatusCode.BadRequest, new Exception("Invalid domain parameter"));
        			}
        		} else {
        			plan.Domain = Domain.FromIdentifier(context, "terradue");
        		}

        		//check if email is already used
        		try {
        			UserT2.FromEmail(context, request.Email);
        			throw new Exception("Sorry, this email is already used.");
        		} catch (Exception) { }

        		var validusername = UserT2.MakeUsernameValid(request.Username);
                AuthenticationType AuthType = IfyWebContext.GetAuthenticationType(typeof(Authentication.Ldap.LdapAuthenticationType));

                var user = UserT2.Create(context, validusername, request.Email, request.Password, AuthType, AccountStatusType.Enabled, true, request.EoSSO, request.Originator, true);

        		//Set plan
        		if (!string.IsNullOrEmpty(request.Plan)) {
        			user.Upgrade(plan);
        		}

        		result = new WebUserT2(user);

        		context.Close();
        	} catch (Exception e) {
        		context.LogError(this, e.Message + " - " + e.StackTrace);
        		context.Close();
        		throw e;
        	}
        	return result;
        }

    }
}

