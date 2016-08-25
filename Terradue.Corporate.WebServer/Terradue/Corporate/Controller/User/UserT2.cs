using System;
using Terradue.Portal;
using Terradue.OpenNebula;
using Terradue.Github;
using Terradue.Util;
using Terradue.Cloud;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
using Terradue.Ldap;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Terradue.Corporate.Controller {
    [EntityTable(null, EntityTableConfiguration.Custom, Storage = EntityTableStorage.Above)]
    public class UserT2 : User {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Json2LdapFactory LdapFactory { get; set; }
        private PlanFactory PlanFactory { get; set; }
        private CatalogueFactory CatFactory { get; set; }
        private ArtifactoryFactory JFrogFactory { get; set; }

        private IUSER oneuser { get; set; }
        public IUSER OneUser { 
            get { 
                if (oneuser == null) {
                    if (oneId == 0) {
                        try {
                            USER_POOL oneUsers = oneClient.UserGetPoolInfo();
                            foreach (object item in oneUsers.Items) {
                                if (item is USER_POOLUSER) {
                                    USER_POOLUSER oneUser = item as USER_POOLUSER;
                                    if (oneUser.NAME == this.Username) {
                                        oneId = Int32.Parse(oneUser.ID);
                                        oneuser = oneUser;
                                        break;
                                    }
                                }
                            }
                        } catch (Exception e) {
                            return null;
                        }
                    } else {
                        USER oneUser = oneClient.UserGetInfo(oneId);
                        oneuser = oneUser;
                    }
                }
                return oneuser;
            }
            set {
                
            }
        }
        
        /// <summary>
        /// Gets or sets the one password.
        /// </summary>
        /// <value>The one password.</value>
        public string OnePassword { 
            get { 
                if (this.IsPaying()) {
                    return OneUser.PASSWORD;
                }
                return null;
            } 
            set {
                onepwd = value;
            } 
        }

        private List<String> onegroups { get; set; }
        public List<String> OneGroups {
            get{ 
                if (onegroups == null) {
                    onegroups = new List<string>();
                    foreach (var group in OneUser.GROUPS) {
                        var gId = Int32.Parse(group);
                        if(gId > 0) onegroups.Add(oneClient.GroupGetInfo(gId).NAME);
                    }
                }
                return onegroups;
            }
        }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        /// <value>The public key.</value>
        public string PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the private key.
        /// </summary>
        /// <value>The private key.</value>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>The API key.</value>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the eosso username.
        /// </summary>
        /// <value>The eo SS.</value>
        public string EoSSO { get; set; }

        private string onepwd { get; set; }

        private int oneId { get; set; }

        private OneClient oneClient { get; set; }

        private Json2LdapClient Json2Ldap { get; set; }

        private Plan plan { get; set; }

        public Plan Plan { 
            get {
                if (plan == null && this.Id != 0) {
                    plan = this.PlanFactory.GetPlanForUser(this.Id);
                }
                return plan;
            }
        }

        [EntityDataField("id_domain")]
        public new int DomainId { get; set; }

        //--------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.Controller.UserT2"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public UserT2(IfyContext context) : base(context) {
            OneCloudProvider oneCloud = (OneCloudProvider)CloudProvider.FromId(context, context.GetConfigIntegerValue("One-default-provider"));
            this.oneClient = oneCloud.XmlRpc;
            this.LdapFactory = new Json2LdapFactory(context);
            this.Json2Ldap = LdapFactory.Json2Ldap;
            this.PlanFactory = new PlanFactory(context);
            this.CatFactory = new CatalogueFactory(context);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.Controller.UserT2"/> class.
        /// </summary>
        /// <param name="user">User.</param>
        public UserT2(IfyContext context, User user) : this(context) {
            this.Id = user.Id;
            this.Load();

            this.Username = user.Username;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Email = user.Email;
            this.Affiliation = user.Affiliation;
            this.Country = user.Country;
            this.Level = user.Level;
        }

        //--------------------------------------------------------------------------------------------------------------

        public UserT2(IfyContext context, LdapUser user) : this(context) {
            this.Username = user.Username;
            this.Load();

            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Email = user.Email;
        }

        //--------------------------------------------------------------------------------------------------------------

        public override string AlternativeIdentifyingCondition {
            get { 
                if (!string.IsNullOrEmpty(Username))
                    return String.Format("t.username='{0}'", Username);
                else if (!string.IsNullOrEmpty(Email))
                    return String.Format("t.email='{0}'", Email); 
                else return null;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new User instance representing the user with the specified ID.
        /// </summary>
        /// <returns>The identifier.</returns>
        /// <param name="context">Context.</param>
        /// <param name="id">Identifier.</param>
        public new static UserT2 FromId(IfyContext context, int id) {
            UserT2 user = new UserT2(context);
            user.Id = id;
            user.Load();
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new User instance representing the user with the specified unique name.
        /// </summary>
        /// <returns>The username.</returns>
        /// <param name="context">Context.</param>
        /// <param name="username">Username.</param>
        public new static UserT2 FromUsername(IfyContext context, string username) {
            UserT2 user = new UserT2(context);
            user.Identifier = username;
            user.Load();
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        public new static UserT2 FromEmail(IfyContext context, string email) {
            UserT2 user = new UserT2(context);
            user.Email = email;
            user.Load();
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <returns>The token.</returns>
        public string GetToken() {
            var token = base.GetActivationToken();
            if (token == null) {
                CreateActivationToken();
                token = base.GetActivationToken();
            }
            return token;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether this instance is a paying user.
        /// </summary>
        /// <returns><c>true</c> if this instance is paying; otherwise, <c>false</c>.</returns>
        public bool IsPaying() {
            return (this.DomainId != 0);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether this instance has github profile.
        /// </summary>
        /// <returns><c>true</c> if this instance has github profile; otherwise, <c>false</c>.</returns>
        public bool HasGithubProfile() {
            try {
                GithubProfile.FromId(context, this.Id);
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether this instance has cloud account.
        /// </summary>
        /// <returns><c>true</c> if this instance has cloud account; otherwise, <c>false</c>.</returns>
        public bool HasCloudAccount() {
            return context.GetQueryBooleanValue(String.Format("SELECT username IS NOT NULL FROM usr_cloud WHERE id={0};", this.Id));
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Upgrade with the specified plan.
        /// </summary>
        /// <param name="plan">Plan.</param>
        public void Upgrade(Plan plan) {
            log.Info(String.Format("Upgrade user {0} with plan {1}", this.Username, plan.Name));
            context.StartTransaction();

            //sanity check
            if (!HasGithubProfile()) CreateGithubProfile();
            if (this.DomainId == 0) CreateDomain();

            switch (plan.Name) {
                case Plan.NONE:
                    break;
                case Plan.TRIAL:
                    
                    break;
                case Plan.EXPLORER:
                case Plan.SCALER:
                case Plan.PREMIUM:
                    if (!HasCloudAccount()) CreateCloudAccount(plan);
                    if (!HasLdapDomain()) CreateLdapDomain();
                    if (!HasCatalogueIndex()) CreateCatalogueIndex();
                    if (!HasRepository()) CreateRepository();
                    break;
                default:
                    break;
            }

            this.PlanFactory.UpgradeUserPlan(this.Id, plan);

            context.Commit();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Stores the password.
        /// </summary>
        /// <param name="pwd">Pwd.</param>
        public new void StorePassword(string pwd) {
            //password check
            ValidatePassword(pwd);
            base.StorePassword(pwd);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Validates the password.
        /// </summary>
        /// <param name="pwd">Pwd.</param>
        public static void ValidatePassword(string pwd) {
            if (pwd.Length < 8)
                throw new Exception("Invalid password: You must use at least 8 characters");
            if (!Regex.Match(pwd, @"[A-Z]").Success)
                throw new Exception("Invalid password: You must use at least one upper case value");
            if (!Regex.Match(pwd, @"[a-z]").Success)
                throw new Exception("Invalid password: You must use at least one lower case value");
            if (!Regex.Match(pwd, @"[\d]").Success)
                throw new Exception("Invalid password: You must use at least one numerical value");
            if (!Regex.Match(pwd, @"[!#@$%^&*()_+]").Success)
                throw new Exception("Invalid password: You must use at least one special character");
            if (!Regex.Match(pwd, @"^[a-zA-Z0-9!#@$%^&*()_+]+$").Success)
                throw new Exception("Invalid password: You password contains illegal characters");
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the github profile.
        /// </summary>
        public void CreateGithubProfile() {
            GithubProfile github = new GithubProfile(context, this.Id);
            github.Store();
        }

        //--------------------------------------------------------------------------------------------------------------

        #region ONE

        /// <summary>
        /// Creates the cloud profile.
        /// </summary>
        public void CreateCloudAccount(Plan plan) {
            log.Info(String.Format("Creating Cloud account for {0}", this.Username));
            if (this.Username.Equals(this.Email)) {
                log.Error(String.Format("Username not set (equal to email)"));
                throw new Exception("Please set a valid username before creating the Cloud account");
            }
            EntityList<CloudProvider> provs = new EntityList<CloudProvider>(context);
            provs.Load();
            foreach (CloudProvider prov in provs) {
                context.Execute(String.Format("DELETE FROM usr_cloud WHERE id={0} AND id_provider={1};", this.Id, prov.Id));
                context.Execute(String.Format("INSERT IGNORE INTO usr_cloud (id, id_provider, username) VALUES ({0},{1},{2});", this.Id, prov.Id, StringUtils.EscapeSql(this.Username)));
            }

            if (GetCloudUser() == null) {

                //create user (using email as password)
                int id = oneClient.UserAllocate(this.Username, this.Email, "SSO");
            }
        }

        public void UpdateCloudAccount(Plan plan){
            var usercloud = GetCloudUser();
            if(usercloud != null) UpdateOneUser(usercloud, plan);
        }

        /// <summary>
        /// Gets the cloud user.
        /// </summary>
        /// <returns>The cloud user.</returns>
        public USER_POOLUSER GetCloudUser(){
            //get user from username
            USER_POOL users = oneClient.UserGetPoolInfo();
            foreach (object user in users.Items) {
                if (user.GetType() == typeof(USER_POOLUSER)) {
                    if (((USER_POOLUSER)user).NAME.Equals(this.Username)) {
                        return (USER_POOLUSER)user;
                    }
                }
            }
            return null;
        }

        private void UpdateOneUser(object user, Plan plan){
            var views = "";
            var defaultview = "";
            switch (plan.Name) {
                case Plan.EXPLORER:
                    views = Plan.EXPLORER;
                    defaultview = Plan.EXPLORER;
                    break;
                case Plan.SCALER:
                    views = Plan.SCALER;
                    defaultview = Plan.SCALER;
                    break;
                case Plan.PREMIUM:
                    views = Plan.SCALER + "," + Plan.PREMIUM;
                    defaultview = Plan.PREMIUM;
                    break;
                default:
                    break;
            }

            List<KeyValuePair<string, string>> templatePairs = new List<KeyValuePair<string, string>>();
            templatePairs.Add(new KeyValuePair<string, string>("USERNAME", this.Username));
            templatePairs.Add(new KeyValuePair<string, string>("SUNSTONE_VIEWS", views));
            templatePairs.Add(new KeyValuePair<string, string>("DEFAULT_VIEW", defaultview));

            XmlNode[] template = null;
            int id = 0;

            if (user is USER) {
                template = (XmlNode[])((user as USER).TEMPLATE);
                id = Int32.Parse((user as USER).ID);
            } else if (user is USER_POOLUSER) {
                template = (XmlNode[])((user as USER_POOLUSER).TEMPLATE);
                id = Int32.Parse((user as USER_POOLUSER).ID);
            }

            if (template != null && id != 0) {
                //update user template
                string templateUser = CreateTemplate(template, templatePairs);
                if (!oneClient.UserUpdate(id, templateUser)) throw new Exception("Error during update of user");
            }
        }

        public void UpdateOneGroup(int grpId){
            var usercloud = GetCloudUser();
            if(usercloud != null) oneClient.UserUpdateGroup(Int32.Parse(usercloud.ID), grpId);
        }

        private string CreateTemplate(XmlNode[] template, List<KeyValuePair<string, string>> pairs){
            List<KeyValuePair<string, string>> originalTemplate = new List<KeyValuePair<string, string>>();
            List<KeyValuePair<string, string>> resultTemplate = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < template.Length; i++) {
                originalTemplate.Add(new KeyValuePair<string, string>(template[i].Name, template[i].InnerText));
            }

            foreach (KeyValuePair<string, string> original in originalTemplate) {
                bool exists = false;
                foreach (KeyValuePair<string, string> pair in pairs) {
                    if (original.Key.Equals(pair.Key)) {
                        exists = true;
                        break;
                    }
                }
                if (!exists) pairs.Add(original);
            }

            string templateUser = "<TEMPLATE>";
            foreach(KeyValuePair<string, string> pair in pairs){
                templateUser += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
            }
            templateUser += "</TEMPLATE>";
            return templateUser;
        }

        public void DeleteCloudAccount(){
            var usercloud = GetCloudUser();
            if(usercloud != null) oneClient.UserDelete(Int32.Parse(usercloud.ID));
            context.Execute(String.Format("DELETE FROM usr_cloud WHERE id={0} AND id_provider={1};", this.Id, context.GetConfigIntegerValue("One-default-provider")));
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the domain.
        /// </summary>
        public void CreateDomain() {
            Domain domain = new Domain(context);
            domain.Identifier = this.Username;
            domain.Name = this.Username;
            domain.Description = string.Format("Domain belonging to user {0}", this.Username);
            domain.Store();
            this.DomainId = domain.Id;
            this.Store();
        }

        //--------------------------------------------------------------------------------------------------------------

        #region SAFE

        /// <summary>
        /// Creates the safe.
        /// </summary>
        public void CreateSafe() {
            Safe safe = new Safe(context);
            safe.GenerateKeys();
            this.PublicKey = safe.GetBase64SSHPublicKey();
            this.PrivateKey = safe.GetBase64SSHPrivateKeyOpenSSL();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region LDAP

        /// <summary>
        /// To LDAP user.
        /// </summary>
        /// <returns>The LDAP user.</returns>
        public LdapUser ToLdapUser() {
            LdapUser user = new LdapUser();
            user.Username = this.Username;
            user.Email = this.Email;
            user.FirstName = this.FirstName;
            user.LastName = this.LastName;
            user.Name = string.Format("{0} {1}", this.FirstName, this.LastName);
            user.PublicKey = this.PublicKey;
            user.EoSSO = this.EoSSO;
            user.ApiKey = this.ApiKey;
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP Distinguished Name for people
        /// </summary>
        /// <returns>The LDAP DN.</returns>
        private string CreateLdapDNforPeople() {
            return LdapFactory.CreateLdapPeopleDN(this.Username);
        }

        /// <summary>
        /// Creates the LDAP Distinguished Name for domain.
        /// </summary>
        /// <returns>The LDAP D nfor domain.</returns>
        private string CreateLdapDNforDomain(){
            string dn = string.Format("cn={0}.owner, ou={0}, ou=domains, dc=terradue, dc=com", this.Username);
            dn = LdapFactory.NormalizeLdapDN(dn);
            return dn;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP account.
        /// </summary>
        public void CreateLdapAccount(string password) {
            
            //open the connection
            Json2Ldap.Connect();
            try {

                string dn = CreateLdapDNforPeople();
                LdapUser ldapusr = this.ToLdapUser();
                ldapusr.DN = dn;
                ldapusr.Password = GenerateSaltedSHA1(password);
                this.GenerateApiKey(password);
                ldapusr.ApiKey = this.ApiKey;

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
                Json2Ldap.AddEntry(ldapusr);
//                Json2Ldap.ModifyPassword(dn, password, password);

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP account.
        /// </summary>
        public void ChangeLdapPassword(string newpassword, string oldpassword = null, bool admin = false) {

            //open the connection
            Json2Ldap.Connect();
            try {

                string dn = CreateLdapDNforPeople();

                if (admin) Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
                else Json2Ldap.SimpleBind(dn, oldpassword);
                Json2Ldap.ModifyPassword(dn, newpassword, oldpassword);
            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Deletes the LDAP account.
        /// </summary>
        public void DeleteLdapAccount() {
            //open the connection
            Json2Ldap.Connect();
            try {

                string dn = CreateLdapDNforPeople();

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
                Json2Ldap.DeleteEntry(dn);

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Updates the LDAP uid.
        /// </summary>
        public void UpdateUsername() {
            ValidateUsername(this.Username);

            //change username on LDAP
            var dn = LdapFactory.CreateLdapPeopleDN(this.Email);

            //open the connection
            Json2Ldap.Connect();
            try {

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));

                Json2Ldap.ModifyUID(dn, this.Username);

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();

            //Change username on the db
            AuthenticationType authType = IfyWebContext.GetAuthenticationType(typeof(Terradue.Authentication.OAuth.OAuth2AuthenticationType));
            string sql = string.Format("UPDATE usr_auth SET username={0} WHERE id_usr={1} and id_auth={2};",
                                       StringUtils.EscapeSql(this.Username),
                                       this.Id,
                                       authType.Id);
            context.Execute(sql);
            sql = string.Format("UPDATE usr SET username={0} WHERE id={1};",
                                StringUtils.EscapeSql(this.Username),
                                this.Id);
            context.Execute(sql);
        }

        /// <summary>
        /// Updates the LDAP account.
        /// </summary>
        /// <param name="password">Password.</param>
        public void UpdateLdapAccount(string password = null) {

            //open the connection
            Json2Ldap.Connect();

            try {

                string dn = CreateLdapDNforPeople();

                LdapUser ldapusr = this.ToLdapUser();
                ldapusr.DN = dn;

                //simple bind to have creation rights
                if(password == null){
                    Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
                } else {
                    Json2Ldap.SimpleBind(dn, password);
                }

                try {
                    Json2Ldap.ModifyUserInformation(ldapusr);
                } catch (Exception e) {
                    try {
                        //user may not have sshPublicKey | eossoUserid | apiKey
                        if (e.Message.Contains("sshPublicKey") || e.Message.Contains("sshUsername")) {
                            Json2Ldap.AddNewAttributeString(dn, "objectClass", "ldapPublicKey");
                            Json2Ldap.ModifyUserInformation(ldapusr);
                        } else if(e.Message.Contains("eossoUserid")){
                            Json2Ldap.AddNewAttributeString(dn, "objectClass", "eossoAccount");
                            Json2Ldap.ModifyUserInformation(ldapusr);
                        } else if(e.Message.Contains("telexNumber")){
                            Json2Ldap.AddNewAttributeString(dn, "objectClass", "telexNumber");
                            Json2Ldap.ModifyUserInformation(ldapusr);
                        } throw e;
                    } catch (Exception e2) {
                        throw e2;
                    }
                }

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Validates the username.
        /// </summary>
        /// <param name="username">Username.</param>
        public static void ValidateUsername(string username) {
            if (Regex.IsMatch(username,"^[a-z_][a-z0-9_-]{1,30}[$]?$"))
                return;

            if (username.Length > 32)
                throw new Exception("Invalid Cloud username: It must have a maximum of 32 characters");
            if (!Regex.IsMatch(username, "^[a-z_]"))
                throw new Exception("Invalid Cloud username: It must begin with a lower case letter or an underscore");
            throw new Exception("Invalid Cloud username: You must use only lower case letters, digits, underscores, or dashes");
        }

        /// <summary>
        /// Makes the username valid.
        /// </summary>
        /// <returns>The username valid.</returns>
        /// <param name="username">Username.</param>
        public static string MakeUsernameValid(string username){
            if (string.IsNullOrEmpty(username)) throw new Exception("empty username"); 

            var result = username.ToLower().Replace(" ", "").Replace(".","").Replace("-","").Replace("_","");
            ValidateUsername(result);
            return result;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Deletes the public key.
        /// </summary>
        /// <param name="password">Password.</param>
        public void DeletePublicKey(string password) {
            //open the connection
            Json2Ldap.Connect();
            try {

                string dn = CreateLdapDNforPeople();

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
                Json2Ldap.DeleteAttributeString(dn, "sshPublicKey", null);

                //TODO: delete also from OpenNebula

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Loads the LDAP info.
        /// </summary>
        public void LoadLdapInfo(string password = null) {
            Json2Ldap.Connect();
            try {
                var dn = CreateLdapDNforPeople();
                if(password != null) Json2Ldap.SimpleBind(dn,password);
                var ldapusr = this.Json2Ldap.GetEntry(dn);
                if(ldapusr != null){
                    if(!string.IsNullOrEmpty(ldapusr.Username)) this.Username = ldapusr.Username;
                    if(!string.IsNullOrEmpty(ldapusr.Email)) this.Email = ldapusr.Email;
                    if(!string.IsNullOrEmpty(ldapusr.FirstName)) this.FirstName = ldapusr.FirstName;
                    if(!string.IsNullOrEmpty(ldapusr.LastName)) this.LastName = ldapusr.LastName;
                    if(!string.IsNullOrEmpty(ldapusr.PublicKey)) this.PublicKey = ldapusr.PublicKey;
                    if(!string.IsNullOrEmpty(ldapusr.ApiKey)) this.ApiKey = ldapusr.ApiKey;
                }
            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        public void PublicLoadSshPubKey(string username) {
            Json2Ldap.Connect();
            try {
                var dn = LdapFactory.CreateLdapPeopleDN(username);
                var ldapusr = this.Json2Ldap.GetEntry(dn);
                if(ldapusr != null){
                    if(!string.IsNullOrEmpty(ldapusr.PublicKey)) this.PublicKey = ldapusr.PublicKey;
                }
            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates the API key.
        /// </summary>
        /// <param name="password">Password.</param>
        public void GenerateApiKey(string password){
            this.ApiKey = Guid.NewGuid().ToString();

            //Api Key is saved also on Artifactory
            this.JFrogFactory.AddApiKey(this.ApiKey, this.Username, password);
        }

        /// <summary>
        /// Revokes the API key.
        /// </summary>
        public void RevokeApiKey(string password) {

            //open the connection
            Json2Ldap.Connect();
            try {

                string dn = CreateLdapDNforPeople();

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
                Json2Ldap.DeleteAttributeString(dn, "telexNumber", null);
                this.ApiKey = null;

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();

            //revoke api key also from Artifactory
            JFrogFactory.RevokeApiKey(this.Username, password);
        }

        /// <summary>
        /// Determines whether this instance has LDAP domain.
        /// </summary>
        /// <returns><c>true</c> if this instance has LDAP domain; otherwise, <c>false</c>.</returns>
        public bool HasLdapDomain(){
            Json2Ldap.Connect();
            bool result = false;
            try {
                var dn = CreateLdapDNforDomain();
                result = this.Json2Ldap.GetEntry(dn) != null;
            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
            return result;
        }

        /// <summary>
        /// Creates the LDAP domain.
        /// </summary>
        public void CreateLdapDomain(){

            var owner = this.Username + ".owner";

            //open the connection
            Json2Ldap.Connect();
            try {

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));

                //create first level entry
                string dn = string.Format("ou={0}, ou=domains, dc=terradue, dc=com", this.Username);
                dn = LdapFactory.NormalizeLdapDN(dn);
                ParamsAttributes attributes = new ParamsAttributes();
                attributes.objectClass = new List<string>{ "organizationalUnit", "top" };
                attributes.ou = this.Username;

                Json2Ldap.AddEntry(dn, attributes);

                //create second level entry
                dn = CreateLdapDNforDomain();

                attributes = new ParamsAttributes();
                attributes.objectClass = new List<string>{ "groupOfUniqueNames"};
                attributes.cn = owner;
                attributes.uniqueMember = CreateLdapDNforPeople();
                attributes.description = "Owner of user domain " + this.Username;

                Json2Ldap.AddEntry(dn, attributes);

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();

            //Add group on Artifactory
            JFrogFactory.CreateGroup(owner);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Add the public key.
        /// </summary>
        public void AddPublicKeyAttribute() {
            Json2Ldap.Connect();
            try {

                Json2Ldap.AddNewAttributeString(CreateLdapDNforPeople(), "objectClass", "ldapPublicKey");

            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
        }

        public static string GenerateSaltedSHA1(string plainTextString)
        {
            HashAlgorithm algorithm = new SHA1Managed();
            var saltBytes = GenerateSalt(4);
            var plainTextBytes = Encoding.ASCII.GetBytes(plainTextString);

            var plainTextWithSaltBytes = AppendByteArray(plainTextBytes, saltBytes);
            var saltedSHA1Bytes = algorithm.ComputeHash(plainTextWithSaltBytes);
            var saltedSHA1WithAppendedSaltBytes = AppendByteArray(saltedSHA1Bytes, saltBytes);

            return "{SSHA}" + Convert.ToBase64String(saltedSHA1WithAppendedSaltBytes);
        } 


        private static byte[] GenerateSalt(int saltSize)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[saltSize];
            rng.GetBytes(buff);
            return buff; 
        }

        private static byte[] AppendByteArray(byte[] byteArray1, byte[] byteArray2)
        {
            var byteArrayResult =
                new byte[byteArray1.Length + byteArray2.Length];

            for (var i = 0; i < byteArray1.Length; i++)
                byteArrayResult[i] = byteArray1[i];
            for (var i = 0; i < byteArray2.Length; i++)
                byteArrayResult[byteArray1.Length + i] = byteArray2[i];

            return byteArrayResult;
        }
       
        #endregion

        #region Catalogue

        public bool HasCatalogueIndex(){
            return this.CatFactory.IndexExists(this.Username);
        }

        public void CreateCatalogueIndex(){
            this.CatFactory.CreateIndex(this.Username);
        }

        public void CreateCatalogueIndex(string index){
            this.CatFactory.CreateIndex(index);
        }

        public List<string> GetUserCatalogueIndexes(){
            if (this.HasCatalogueIndex())
                return new List<string>{ this.Username };
            else 
                return null;
        }

        #endregion

        #region Artifactory

        public bool HasRepository(){
            return this.JFrogFactory.RepositoryExists(this.Username);
        }

        public void CreateRepository(){
            this.CreateRepository(this.Username);
        }

        public void CreateRepository(string repo){
            this.JFrogFactory.CreateLocalRepository(repo);

            //sanity check: does user has domain on ldap ?
            if (!HasLdapDomain()) CreateLdapDomain();

            //Create permission for groups on new repo
            var group = this.Username + ".owner";
            this.JFrogFactory.CreatePermissionForGroupOnRepo(group, repo);
        }

        public List<string> GetUserRepositories(){
            if (this.HasRepository())
                return new List<string>{ this.Username };
            else 
                return null;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        public void ValidateNewEmail(string email){

            //simple checks on the email
            if(string.IsNullOrEmpty(email)) throw new Exception("Your new email is empty.");
            if(!email.Contains("@")) throw new Exception("Invalid email.");

            //check the email is not already used on LDAP
            if(LdapFactory.GetUserFromEmail(email) != null) throw new Exception("This email is already used.");
        }

    }
}

