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
using Terradue.Certification.WebService;

namespace Terradue.Corporate.WebServer {

    [Route("/user/{id}", "GET", Summary = "GET the user", Notes = "User is found from id")]
    public class GetUserT2 : IReturn<WebUserT2> {
        [ApiMember(Name = "id", Description = "User id", ParameterType = "query", DataType = "int", IsRequired = true)]
        public int Id { get; set; }
    }

    [Route("/user/current", "GET", Summary = "GET the current user", Notes = "User is the current user")]
    public class GetCurrentUserT2 : IReturn<WebUserT2> {}

    [Route("/user", "PUT", Summary = "Update user", Notes = "User is contained in the PUT data. Only non UMSSO data can be updated, e.g redmineApiKey or certField")]
    public class UpdateUserT2 : WebUserT2, IReturn<WebUserT2> {}

    [Route("/user", "POST", Summary = "Create a new user", Notes = "User is contained in the POST data.")]
    public class CreateUserT2 : WebUserT2, IReturn<WebUserT2> {}

    [Route("/user/cert", "PUT", Summary = "Update user cert", Notes = "User is contained in the PUT data. Only non UMSSO data can be updated, e.g redmineApiKey or certField")]
    public class UpdateUserCertTep : WebUserT2, IReturn<WebUserT2> {}

    [Route("/user/registration", "POST", Summary = "Register a new user", Notes = "User is contained in the POST data.")]
    public class RegisterUserT2 : WebUserRegistrationT2, IReturn<WebUserT2> {}

    [Route("/user/upgrade", "POST", Summary = "Upgrade a user", Notes = "User is contained in the POST data.")]
    public class UpgradeUserT2 : WebUserUpgradeT2, IReturn<WebUserT2> {}

    [Route("/user/safe", "POST", Summary = "create a safe for user", Notes = "User is contained in the POST data.")]
    public class CreateSafeUserT2 : WebUserT2, IReturn<WebUserT2> {}



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

    public class WebUserUpgradeT2 : WebUser{
        
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

        [ApiMember(Name = "onepassword", Description = "User password on OpenNebula", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String OnePassword { get; set; }

        [ApiMember(Name = "DomainId", Description = "User domain id", ParameterType = "query", DataType = "int", IsRequired = false)]
        public int DomainId { get; set; }

        [ApiMember(Name = "EmailNotification", Description = "User email notification tag", ParameterType = "query", DataType = "bool", IsRequired = false)]
        public bool EmailNotification { get; set; }

        [ApiMember(Name = "PublicKey", Description = "User PublicKey", ParameterType = "query", DataType = "String", IsRequired = false)]
        public String PublicKey { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        public WebUserT2() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.WebServer.WebUserT2"/> class.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public WebUserT2(UserT2 entity) : base(entity) {
            this.OnePassword = entity.OnePassword;
            this.DomainId = entity.DomainId;
            if (entity.HasSafe()) {
                this.PublicKey = entity.GetPublicKey();
            }
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

