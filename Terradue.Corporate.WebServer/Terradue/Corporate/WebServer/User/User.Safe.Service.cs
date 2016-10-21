using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using Terradue.Corporate.Controller;
using Terradue.Ldap;

namespace Terradue.Corporate.WebServer
{
    [Api ("Terradue Corporate webserver")]
    [Restrict (EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserSafeService : ServiceStack.ServiceInterface.Service
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post (CreateSafeUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext (PagePrivileges.UserView);
            WebSafe result;
            try {
                context.Open ();

                Connect2IdClient client = new Connect2IdClient (context.GetConfigValue ("sso-configUrl"));
                client.SSOAuthEndpoint = context.GetConfigValue ("sso-authEndpoint");
                client.SSOApiClient = context.GetConfigValue ("sso-clientId");
                client.SSOApiSecret = context.GetConfigValue ("sso-clientSecret");
                client.SSOApiToken = context.GetConfigValue ("sso-apiAccessToken");

                UserT2 user = UserT2.FromId (context, context.UserId);

                log.InfoFormat ("Create safe for user {0}", user.Username);

                //authenticate user
                try {
                    var j2ldapclient = new LdapAuthClient (context.GetConfigValue ("ldap-authEndpoint"));
                    var usr = j2ldapclient.Authenticate (user.Identifier, request.password, context.GetConfigValue ("ldap-apikey"));
                } catch (Exception e) {
                    log.ErrorFormat ("Error during safe creation - {0} - {1}", e.Message, e.StackTrace);
                    throw new Exception ("Invalid password");
                }

                user.CreateSafe ();
                log.InfoFormat ("Safe created locally");
                user.UpdateLdapAccount ();
                log.InfoFormat ("Safe saved on ldap");

                result = new WebSafe ();
                result.PublicKey = user.PublicKey;
                result.PrivateKey = user.PrivateKey;

                context.Close ();
            } catch (Exception e) {
                log.ErrorFormat ("Error during safe creation - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return result;
        }

        public object Delete (DeleteSafeUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext (PagePrivileges.UserView);

            try {
                context.Open ();
                UserT2 user = UserT2.FromId (context, context.UserId);
                log.InfoFormat ("Delete safe for user {0}", user.Username);
                //authenticate user
                try {
                    var j2ldapclient = new LdapAuthClient (context.GetConfigValue ("ldap-authEndpoint"));
                    var usr = j2ldapclient.Authenticate (user.Identifier, request.password, context.GetConfigValue ("ldap-apikey"));
                } catch (Exception e) {
                    log.ErrorFormat ("Error during safe delete - {0} - {1}", e.Message, e.StackTrace);
                    throw new Exception ("Invalid password");
                }
                user.DeletePublicKey (request.password);
                log.InfoFormat ("Safe deleted successfully");
                context.Close ();
            } catch (Exception e) {
                log.ErrorFormat ("Error during safe delete - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return new WebResponseBool (true);
        }

    }

}

