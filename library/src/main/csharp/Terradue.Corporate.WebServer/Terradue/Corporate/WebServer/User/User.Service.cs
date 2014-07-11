using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.OpenNebula;
using Terradue.Security.Certification;
using Terradue.TepQW.WebServer;
using Terradue.Corporate.Controller;

namespace Terradue.Corporate.WebServer {
    [Api("Tep-QuickWin Terradue webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserService : ServiceStack.ServiceInterface.Service {
        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(GetUserT2 request) {
            WebUserT2 result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, request.Id);
                result = new WebUserT2(user);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Get the current user.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns>the current user</returns>
        public object Get(GetCurrentUserT2 request) {
            WebUserT2 result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            context.ConsoleDebug = true;
            try {
                context.Open();
                UserT2 user = UserT2.FromId(context, context.UserId);
                result = new WebUserT2(user);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Get the list of all users.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns>the users</returns>
        public object Get(GetUsers request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            List<WebUser> result = new List<WebUser>();
            try {
                context.Open();

                EntityList<User> users = new EntityList<User>(context);
                users.Load();
                foreach(User u in users) result.Add(new WebUser(u));

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Update the specified user.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns>the user</returns>
        public object Put(UpdateUserT2 request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebUserT2 result;
            try {
                context.Open();
                UserT2 user = request.ToEntity(context);
                user.Store();
                result = new WebUserT2(user);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(CreateUserT2 request)
        {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebUserT2 result;
            try{
                context.Open();
                UserT2 user = request.ToEntity(context);
                if(request.Id != 0 && context.UserLevel == UserLevel.Administrator){
                    user.AccountStatus = AccountStatusType.Enabled;
                }
                else{
                    user.AccountStatus = AccountStatusType.PendingActivation;
                }

                user.IsNormalAccount = true;
                user.Level = UserLevel.User;

                user.Store();

                result = new WebUserT2(user);
                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Delete the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Delete(DeleteUser request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                User user = User.FromId(context, request.Id);
                if (context.UserLevel == UserLevel.Administrator) user.Delete();
                else throw new UnauthorizedAccessException(CustomErrorMessages.ADMINISTRATOR_ONLY_ACTION);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return true;
        }

        public object Put(UpdateUserCertT2 request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                CertificateUser certUser = CertificateUser.FromId(context,request.Id);
                //certUser.CertificateSubject = request.CertSubject;
                certUser.Store();
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return true;
        }

        public object Post(RequestCertificate request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            CertificateUser certUser;
            WebUserCertificate userCert;

            try {
                context.Open();
                certUser = (CertificateUser)CertificateUser.FromId(context, context.UserId);

            } catch (EntityNotFoundException e) {
                certUser = new CertificateUser(context);
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            try {
                certUser.RequestCertificate(request.password);
                userCert = new WebUserCertificate(certUser);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return null;
            //return userCert;
        }

        public object Delete(DeleteUserCertificate request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            UserT2 user;

            try {
                context.Open();
                user = UserT2.FromId(context, context.UserId);

            } catch (Exception e) {
                context.Close();
                throw e;
            }

            try {
                user.RemoveCertificate();
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return true;
        }
    }
}

