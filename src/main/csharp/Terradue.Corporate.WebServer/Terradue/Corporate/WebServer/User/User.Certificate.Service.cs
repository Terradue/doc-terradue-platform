using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;
using ServiceStack.ServiceInterface;
using Terradue.Corporate.Controller;
using Terradue.Security.Certification;

namespace Terradue.Corporate.WebServer {
    [Api("Tep-QuickWin Terradue webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class UserCertificateService : ServiceStack.ServiceInterface.Service {

        /// <summary>
        /// Post the specified request (upload certificate).
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(UploadCertificate request)
        {
            CertificateUser user;
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebUserCertificate userCert;
            try {
                context.Open();
                user = CertificateUser.FromId (context,context.UserId);
                user.StoreCertificate(request.RequestStream);

                userCert = new WebUserCertificate(user);
            }catch(Exception e)
            {
                context.Close ();
                throw e;
            }
            return userCert;
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
            //return null;
            return userCert;
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

            return new WebResponseBool(true);
        }

        public object Get(GetUserCertificate request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            CertificateUser certUser;
            WebUserCertificate userCert;

            try {
                context.Open();
                certUser = CertificateUser.FromId(context, context.UserId);
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            try {
                if (certUser.IsUnderApproval()) certUser.TryDownloadAndStoreCertificate();
            } catch (ResourceNotFoundException e){

            }

            try {
                userCert = new WebUserCertificate(certUser);
            } catch (Exception e) {
                context.Close();
                throw e;
            }

            context.Close();

            return userCert;
        }
    }
}

