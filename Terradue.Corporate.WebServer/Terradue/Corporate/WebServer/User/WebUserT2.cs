using System;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.ServiceInterface;
using Terradue.Portal;
using Terradue.WebService.Model;
using Terradue.Corporate.Controller;

namespace Terradue.Corporate.WebServer {
    
    [Route("/user/{id}", "GET", Summary = "GET the user", Notes = "User is found from id")]
    public class GetUserT2 : IReturn<WebUserT2> {
        [ApiMember(Name = "id", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    [Route("/user/{id}/admin", "GET", Summary = "GET the user", Notes = "User is found from id")]
    public class GetUserT2ForAdmin : IReturn<WebUserT2> {
        [ApiMember(Name = "id", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    [Route("/user/ldap", "GET", Summary = "GET the users", Notes = "User is found on ldap")]
    public class GetLdapUsers : IReturn<WebUserT2> {
    }

    [Route ("/user/ldap", "POST", Summary = "POST the user ldap", Notes = "User is found on ldap")]
    public class CreateLdapAccount : IReturn<WebUserT2>{
        [ApiMember (Name = "password", Description = "User password", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Password { get; set; }
    }

    [Route("/user/{username}", "GET", Summary = "GET the user", Notes = "User is found from username")]
    public class GetUserNameT2 : IReturn<WebUserT2> {
        [ApiMember(Name = "username", Description = "User identifier", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Username { get; set; }
    }

    [Route("/user/passwordreset", "POST", Summary = "PUT the user password to reset", Notes = "User is found from username")]
    public class ResetPassword : IReturn<WebResponseBool> {
        [ApiMember(Name = "username", Description = "User name", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Username { get; set; }
    }

    [Route("/user/passwordreset", "PUT", Summary = "PUT the user password to reset", Notes = "User is found from username")]
    public class UserResetPassword : IReturn<WebResponseBool> {
        [ApiMember(Name = "username", Description = "User name", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Username { get; set; }

        [ApiMember(Name = "password", Description = "User password", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Password { get; set; }

        [ApiMember(Name = "token", Description = "User token", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Token { get; set; }
    }

    [Route("/user/password", "PUT", Summary = "PUT the user password to reset", Notes = "User is found from username")]
    public class UserUpdatePassword : IReturn<WebResponseBool> {

        [ApiMember(Name = "newpassword", Description = "User new password", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string NewPassword { get; set; }

        [ApiMember(Name = "oldpassword", Description = "User old password", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string OldPassword { get; set; }
    }

    [Route("/user/current", "GET", Summary = "GET the current user", Notes = "User is the current user")]
    public class GetCurrentUserT2 : IReturn<WebUserT2> {
        [ApiMember (Name = "ldap", Description = "get also ldap info", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool ldap { get; set; }
    }

    [Route ("/user/current/logstatus", "GET", Summary = "GET the status of the current user", Notes = "true = is logged, false = is not logged")]
    public class UserCurrentIsLoggedRequest : IReturn<WebResponseBool>
    {
    }

    [Route("/user", "PUT", Summary = "Update user", Notes = "User is contained in the PUT data. Only non UMSSO data can be updated, e.g redmineApiKey or certField")]
    public class UpdateUserT2 : WebUserT2, IReturn<WebUserT2> {}

    [Route("/user", "POST", Summary = "Create a new user", Notes = "User is contained in the POST data.")]
    public class CreateUserT2 : WebUserT2, IReturn<WebUserT2> {}

    [Route ("/user/username", "POST", Summary = "Update user username", Notes = "User is contained in the PUT data. Only non UMSSO data can be updated, e.g redmineApiKey or certField")]
    public class UpdateUsernameT2 : WebUserT2, IReturn<WebUserT2> { }

    [Route("/user/cert", "PUT", Summary = "Update user cert", Notes = "User is contained in the PUT data. Only non UMSSO data can be updated, e.g redmineApiKey or certField")]
    public class UpdateUserCertTep : WebUserT2, IReturn<WebUserT2> {}

    [Route("/user/registration", "POST", Summary = "Register a new user", Notes = "User is contained in the POST data.")]
    public class RegisterUserT2 : WebUserRegistrationT2, IReturn<WebUserT2> {}

    [Route("/user/upgrade", "POST", Summary = "Upgrade a user", Notes = "User is contained in the POST data.")]
    public class UpgradeUserT2 : WebUserUpgradeT2, IReturn<WebUserT2> {}

    [Route("/private/user/info", "GET", Summary = "get private info for user", Notes = "Username is contained in the GET data.")]
    public class GetPrivateUserInfoT2 : IReturn<WebSafe> {
        [ApiMember(Name = "username", Description = "Username", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string username { get; set; }

        [ApiMember(Name = "token", Description = "token", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string token { get; set; }

        [ApiMember(Name = "request", Description = "token", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string request { get; set; }
    }

    [Route("/user/safe", "POST", Summary = "create a safe for user", Notes = "User is contained in the POST data.")]
    public class CreateSafeUserT2 : IReturn<WebSafe> {
        [ApiMember(Name = "password", Description = "User id", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/user/safe", "DELETE", Summary = "delete a safe for user", Notes = "User is the current user.")]
    public class DeleteSafeUserT2 : IReturn<WebSafe> {
        [ApiMember(Name = "password", Description = "User id", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/user/safe", "PUT", Summary = "recreate a safe for user", Notes = "")]
    public class ReCreateSafeUserT2 : IReturn<WebSafe> {
        [ApiMember(Name = "password", Description = "User id", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/user/apikey", "GET", Summary = "get api key for user", Notes = "")]
    public class GetApiKeyUserT2 : IReturn<string> {
        [ApiMember(Name = "password", Description = "user password", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/user/apikey", "PUT", Summary = "recreate an API Key for user", Notes = "")]
    public class ReGenerateApiKeyUserT2 : IReturn<string> {
        [ApiMember(Name = "password", Description = "User id", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/user/apikey", "DELETE", Summary = "delete an API Key for user", Notes = "")]
    public class DeleteApiKeyUserT2 {
        [ApiMember(Name = "password", Description = "User id", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/user/catalogue/index", "GET", Summary = "get current user catalogue indexes", Notes = "")]
    public class GetCurrentUserCatalogueIndexes : IReturn<List<string>> {
    }

    [Route("/user/catalogue/index", "POST", Summary = "create catalogue index for current user", Notes = "")]
    public class CreateUserCatalogueIndex : IReturn<List<string>> {
        [ApiMember(Name = "index", Description = "User index", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string index { get; set; }

        [ApiMember (Name = "id", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int Id { get; set; }
    }

    [Route("/user/features", "GET", Summary = "get current user features", Notes = "")]
    public class GetCurrentUserFeatures : IReturn<List<string>> {
    }

    [Route("/user/features/geoserver", "POST", Summary = "create geoserver feature for current user", Notes = "")]
    public class CreateCurrentUserFeatureGeoserver : IReturn<List<string>> {
        [ApiMember(Name = "repo", Description = "User repo", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string repo { get; set; }
    }

    [Route("/user/email", "PUT", Summary = "update email for user", Notes = "")]
    public class UpdateEmailUserT2 : WebUserT2, IReturn<WebUserT2> {
    }

    [Route("/user/safe/private", "PUT", Summary = "get a safe for user", Notes = "")]
    public class GetPrivateSafeUserT2 : IReturn<WebSafe> {
        [ApiMember(Name = "password", Description = "User id", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string password { get; set; }
    }

    [Route("/user/ldap/available", "GET", Summary = "GET if the username is free or not", Notes = "")]
    public class GetAvailableLdapUsernameT2 : IReturn<WebUserT2> {
        [ApiMember(Name = "username", Description = "username", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string username { get; set; }
    }

    [Route("/user/ldap/domain", "GET", Summary = "GET list all domains of the user", Notes = "")]
    public class GetLdapDomains : IReturn<WebUserT2> {
        [ApiMember(Name = "id", Description = "id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    [Route ("/user/ldap/domain", "POST", Summary = "GET list all domains of the user", Notes = "")]
    public class CreateLdapDomain : IReturn<WebUserT2>
    {
        [ApiMember (Name = "id", Description = "id of the user", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------

    public class WebSafe {

        [ApiMember(Name = "PublicKey", Description = "User PublicKey", ParameterType = "query", DataType = "String", IsRequired = true)]
        public String PublicKey { get; set; }

        [ApiMember(Name = "PrivateKey", Description = "User PrivateKey", ParameterType = "query", DataType = "String", IsRequired = true)]
        public String PrivateKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        public WebSafe() {}

    }

    public class WebPlan
    {

        [ApiMember (Name = "Role", Description = "Plan Role", ParameterType = "query", DataType = "WebRole", IsRequired = true)]
        public WebRole Role { get; set; }

        [ApiMember (Name = "Domain", Description = "Plan Domain", ParameterType = "query", DataType = "WebDomain", IsRequired = true)]
        public WebDomain Domain { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        public WebPlan () { }

        public WebPlan (Plan plan) {
            this.Role = new WebRole (plan.Role);
            this.Domain = new WebDomain (plan.Domain);
        }

        public Plan ToEntity (IfyContext context)
        {
            Plan plan = new Plan ();
            Role role = this.Role != null ? Terradue.Portal.Role.FromId (context, this.Role.Id) : null;
            Domain domain = this.Domain != null ? Terradue.Portal.Domain.FromId (context, this.Domain.Id) : null;
            plan.Role = role;
            plan.Domain = domain;
            return plan;
        }

    }


    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------

    public class WebUserRegistrationT2 : WebUser{
        [ApiMember(Name = "captchaValue", Description = "User recaptcha captchaValue", ParameterType = "query", DataType = "String", IsRequired = true)]
        public String captchaValue { get; set; }

        [ApiMember(Name = "captchaPublicKey", Description = "User recaptcha captchaPublicKey", ParameterType = "query", DataType = "String", IsRequired = true)]
        public String captchaPublicKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        public WebUserRegistrationT2() {}

        /// <summary>
        /// Tos the entity.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="context">Context.</param>
        public UserT2 ToEntity(IfyContext context, UserT2 input) {
            UserT2 user = (input == null ? new UserT2(context) : input);
            base.ToEntity(context, user);
            return user;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------

    public class WebUserUpgradeT2 : WebUserT2{
        
        [ApiMember(Name = "Message", Description = "User message", ParameterType = "query", DataType = "String", IsRequired = true)]
        public String Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        public WebUserUpgradeT2() {}

        /// <summary>
        /// Tos the entity.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="context">Context.</param>
        public UserT2 ToEntity(IfyContext context, UserT2 input) {
            UserT2 user = (input == null ? new UserT2(context) : input);
            base.ToEntity(context, user);
            return user;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// User.
    /// </summary>
    public class WebUserT2 : WebUser{

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

        [ApiMember(Name = "hasoneaccount", Description = "Says if user has an account on OpenNebula", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool HasOneAccount { get; set; }

        [ApiMember (Name = "hasldapaccount", Description = "Says if user has an account on LDAP", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool HasLdapAccount { get; set; }

        [ApiMember(Name = "DomainId", Description = "User domain id", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int DomainId { get; set; }

        [ApiMember(Name = "EmailNotification", Description = "User email notification tag", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool EmailNotification { get; set; }

        [ApiMember(Name = "PublicKey", Description = "User PublicKey", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String PublicKey { get; set; }

        [ApiMember(Name = "ApiKey", Description = "User ApiKey", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String ApiKey { get; set; }

        [ApiMember(Name = "Plan", Description = "User Plan", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String Plan { get; set; }
        //[ApiMember(Name = "Plans", Description = "User Plan", ParameterType = "query", DataType = "WebPlan", IsRequired = false)]
        //public List<WebPlan> Plans { get; set; }

        [ApiMember(Name = "HasLdapDomain", Description = "Check if user has ldap domain", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool HasLdapDomain { get; set; }

        [ApiMember(Name = "ArtifactoryDomainSynced", Description = "Check if user has Artifactory domain", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool ArtifactoryDomainSynced { get; set; }

        [ApiMember(Name = "ArtifactoryDomainExists", Description = "Check if user has Artifactory domain", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool ArtifactoryDomainExists { get; set; }

        [ApiMember (Name = "HasCatalogueIndex", Description = "Check if user has catalogue index", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool HasCatalogueIndex { get; set; }

        [ApiMember (Name = "ExternalAuth", Description = "Check if user uses external auth", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool ExternalAuth { get; set; }

        [ApiMember (Name = "FirstLoginDate", Description = "User first login date", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string FirstLoginDate { get; set; }

        [ApiMember (Name = "LastLoginDate", Description = "User last login date", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string LastLoginDate { get; set; }

        [ApiMember (Name = "RegistrationOrigin", Description = "User Registration Origin", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string RegistrationOrigin { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        public WebUserT2() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="ldap">If set to <c>true</c> get also info from LDAP.</param>
        /// <param name="admin">If set to <c>true</c> get all info for admin.</param>
        public WebUserT2(UserT2 entity, bool ldap = false, bool admin = false) : base(entity) {

            log.DebugFormat ("Transforms UserT2 into WebUserT2");

            this.DomainId = entity.DomainId;
            var role = entity.GetRoleForDomain ("terradue");
            this.Plan = role != null ? role.Name : PlanFactory.NONE;
            //this.Plans = new List<WebPlan> ();
            //foreach (var plan in entity.Plans) this.Plans.Add (new WebPlan (plan));

            if (ldap || admin) {
                log.DebugFormat ("Get LDAP info");
                this.HasLdapAccount = entity.HasLdapAccount ();
                if (this.HasLdapAccount) {
                    if (entity.PublicKey == null) entity.LoadLdapInfo ();
                    if (entity.ApiKey == null) entity.LoadApiKey ();
                    if (entity.OneUser != null) {
                        this.HasOneAccount = true;
                    }
                    this.PublicKey = entity.PublicKey;
                    this.ApiKey = entity.ApiKey;
                }
            }
            if (admin) { 
                log.DebugFormat ("Get ADMIN info - HasLDAPDomain");
                this.HasLdapDomain = entity.HasLdapDomain ();
                log.DebugFormat ("Get ADMIN info - ArtifactoryDomainSynced");
                this.ArtifactoryDomainSynced = entity.HasOwnerGroup ();
                log.DebugFormat ("Get ADMIN info - ArtifactoryDomainExists");
                this.ArtifactoryDomainExists = entity.OwnerGroupExists ();
                log.DebugFormat ("Get ADMIN info - HasCatalogueIndex");
                this.HasCatalogueIndex = entity.HasCatalogueIndex ();
                log.DebugFormat ("Get ADMIN info - Last Login date");
                DateTime timef = entity.RegistrationDate == DateTime.MinValue ? entity.GetFirstLoginDate () : entity.RegistrationDate;
                this.FirstLoginDate = (timef == DateTime.MinValue ? null : timef.ToString ("U"));
                DateTime timel = entity.GetLastLoginDate ();
                this.LastLoginDate = (timel == DateTime.MinValue ? null : timel.ToString ("U"));
                this.RegistrationOrigin = entity.RegistrationOrigin;
            }

            this.ExternalAuth = entity.IsExternalAuthentication ();
        }

        /// <summary>
        /// Tos the entity.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="context">Context.</param>
        public UserT2 ToEntity(IfyContext context, UserT2 input) {
            UserT2 user = (input == null ? new UserT2(context) : input);
            base.ToEntity(context, user);
            return user;
        }

    }
}

