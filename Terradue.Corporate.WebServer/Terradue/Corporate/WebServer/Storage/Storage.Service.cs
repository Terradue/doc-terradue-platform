using System;
using ServiceStack.ServiceHost;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.Corporate.Controller;
using Terradue.JFrog.Artifactory;

namespace Terradue.Corporate.WebServer
{
    [Api ("Terradue Corporate webserver")]
    [Restrict (EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class StorageService : ServiceStack.ServiceInterface.Service
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);


        public object Get (GetUserRepositories request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext (PagePrivileges.UserView);
            List<WebStorage> result = new List<WebStorage>();
            try {
                context.Open ();
                UserT2 user = UserT2.FromId (context, request.Id != 0 ? request.Id : context.UserId);
                log.InfoFormat ("Get repositories for user {0}", user.Username);

                foreach (var repo in user.GetUserRepositories ()) {
                    result.Add (new WebStorage (repo));
                }

                context.Close ();
            } catch (Exception e) {
                log.ErrorFormat ("Error during get repositories - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return result;
        }

        public object Post (CreateUserRepository request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext (PagePrivileges.UserView);
            List<WebStorage> result = new List<WebStorage>();
            try {
                context.Open ();
                UserT2 user = UserT2.FromId (context, request.Id != 0 ? request.Id : context.UserId);
                log.InfoFormat ("Create repository '{1}' for user {0}", user.Username, request.repo ?? user.Username);

                user.CreateRepository (request.repo);

                foreach (var repo in user.GetUserRepositories ()) {
                    result.Add (new WebStorage (repo));
                }

                context.Close ();
            } catch (Exception e) {
                log.ErrorFormat ("Error during repository create - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return result;
        }

        public object Get (GetUserRepositoriesGroup request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext (PagePrivileges.AdminOnly);
            List<string> result = null;
            try {
                context.Open ();
                UserT2 user = UserT2.FromId (context, request.Id != 0 ? request.Id : context.UserId);

                log.InfoFormat ("Get repository group list for user {0}", user.Username);

                result = user.GetUserArtifactoryGroups ();

                context.Close ();
            } catch (Exception e) {
                log.ErrorFormat ("Error during repository group get - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return result;
        }

        public object Post (CreateUserRepositoryGroup request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext (PagePrivileges.AdminOnly);
            List<string> result = null;
            try {
                context.Open ();
                UserT2 user = UserT2.FromId (context, request.Id);

                log.InfoFormat ("Create repository group '{1}' for user {0}", user.Username, user.Username + ".owner");

                user.CreateArtifactoryGroup ();
                result = user.GetUserArtifactoryGroups ();

                context.Close ();
            } catch (Exception e) {
                log.ErrorFormat ("Error during create repository group - {0} - {1}", e.Message, e.StackTrace);
                context.Close ();
                throw e;
            }
            return result;
        }

    }

}

