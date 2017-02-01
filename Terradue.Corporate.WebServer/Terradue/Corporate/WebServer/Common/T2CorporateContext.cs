using System;
using System.IO;
using System.Net;
using System.Web;
using Terradue.Portal;

namespace Terradue.Corporate.WebServer.Common {

    /// <summary>
    /// TepQW web context.
    /// </summary>
    public class T2CorporateLocalContext : IfyLocalContext {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Terradue.Corporate.WebServer.Common.T2CorporateLocalContext"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="baseUrl">Base URL.</param>
        /// <param name="applicationName">Application name.</param>
        public T2CorporateLocalContext(string connectionString, string baseUrl, string applicationName) : base(connectionString,baseUrl,applicationName) {}
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Terradue.Corporate.WebServer.Common.T2CorporateLocalContext"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="console">If set to <c>true</c> console.</param>
        public T2CorporateLocalContext(string connectionString, bool console) : base(connectionString,console){}
    }

    /// <summary>
    /// TepQW web context.
    /// </summary>
    public class T2CorporateWebContext : IfyWebContext {


        public T2CorporateWebContext(PagePrivileges privileges) : base(privileges) {
            HideMessages = true;
            System.Configuration.Configuration rootWebConfig =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
            BaseUrl = rootWebConfig.AppSettings.Settings["BaseUrl"].Value;

            HttpContext.Current.Session.Timeout = 1440;
            this.DynamicDbConnectionsGlobal = true;
        }

        public static T2CorporateWebContext GetWebContext(PagePrivileges privileges){
            T2CorporateWebContext result = new T2CorporateWebContext (privileges);
            return result;
        }

        public override void Open (){
            base.Open ();
            if (UserLevel == Terradue.Portal.UserLevel.Administrator) AdminMode = true;
        }

        protected override void LoadAdditionalConfiguration() {
            base.LoadAdditionalConfiguration();
            System.Configuration.Configuration rootWebConfig =
                System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
            BaseUrl = rootWebConfig.AppSettings.Settings["BaseUrl"].Value;
        }

        public override bool CheckCanStartSession(User user, bool throwOnError) {
            if (user.AccountStatus == AccountStatusType.PendingActivation && GetConfigBooleanValue("PendingUserCanLogin")) return true;
            return base.CheckCanStartSession(user, throwOnError);
        }

        public override void LogError (object reporter, string message)
        {
            PrepareLogger (reporter);
            base.LogError (reporter, message);
        }

        public override void LogDebug (object reporter, string message)
        {
            PrepareLogger (reporter);
            base.LogDebug (reporter, message);
        }

        public override void LogInfo (object reporter, string message)
        {
            PrepareLogger (reporter);
            base.LogInfo (reporter, message);
        }

        private void PrepareLogger (object reporter)
        {
            log4net.GlobalContext.Properties ["loguser"] = this.Username != null ? this.Username : "Not logged user";
        }
    }
}

