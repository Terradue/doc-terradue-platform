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

namespace Terradue.Corporate.Controller {
    [EntityTable(null, EntityTableConfiguration.Custom, Storage = EntityTableStorage.Above)]
    public class UserT2 : User {
        
        /// <summary>
        /// Gets or sets the one password.
        /// </summary>
        /// <value>The one password.</value>
        public string OnePassword { 
            get { 
                if (onepwd == null && this.IsPaying()) {
                    if (oneId == 0) {
                        try {
                            USER_POOL oneUsers = oneClient.UserGetPoolInfo();
                            foreach (object item in oneUsers.Items) {
                                if (item is USER_POOLUSER) {
                                    USER_POOLUSER oneUser = item as USER_POOLUSER;
                                    if (oneUser.NAME == this.Email) {
                                        oneId = Int32.Parse(oneUser.ID);
                                        onepwd = oneUser.PASSWORD;
                                        break;
                                    }
                                }
                            }
                        } catch (Exception e) {
                            return null;
                        }
                    } else {
                        USER oneUser = oneClient.UserGetInfo(oneId);
                        onepwd = oneUser.PASSWORD;
                    }
                }
                return onepwd;
            } 
            set {
                onepwd = value;
            } 
        }

        private string onepwd { get; set; }

        private int oneId { get; set; }

        private OneClient oneClient { get; set; }

        private Json2LdapClient Json2Ldap { get; set; }

        private Safe safe { get; set; }

        private Plan plan { get; set; }

        private Plan Plan { 
            get {
                if (plan == null && this.Id != 0) {
                    plan = new Plan(context, this.Id);
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
            this.Json2Ldap = new Json2LdapClient(context.GetConfigValue("ldap-baseurl"));
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

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <returns>The token.</returns>
        public string GetToken(){
			var token = base.GetActivationToken();
			if (token == null) {
				CreateActivationToken ();
				token = base.GetActivationToken ();
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
        /// Determines whether this instance has a safe created.
        /// </summary>
        /// <returns><c>true</c> if this instance has a safe; otherwise, <c>false</c>.</returns>
        public bool HasSafe() {
            return (this.safe != null);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Load this instance.
        /// </summary>
        public override void Load() {
            base.Load();
            try {
                safe = Safe.FromUserId(context, this.Id);
            } catch (Exception) {
                safe = null;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Upgrade the account of the current user
        /// </summary>
        /// <param name="level">Level.</param>
        public void Upgrade(PlanType level) {
            context.StartTransaction();

            if (!HasGithubProfile())
                CreateGithubProfile();
            if (!HasCloudAccount())
                CreateCloudAccount(); //TODO: linked to safe ?
            if (this.DomainId == 0)
                CreateDomain();

            this.Plan.Upgrade(level);

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
        public void ValidatePassword(string pwd) {
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

        /// <summary>
        /// Creates the cloud profile.
        /// </summary>
        public void CreateCloudAccount() {
            EntityList<CloudProvider> provs = new EntityList<CloudProvider>(context);
            provs.Load();
            foreach (CloudProvider prov in provs) {
                context.Execute(String.Format("INSERT IGNORE INTO usr_cloud (id, id_provider, username) VALUES ({0},{1},{2});", this.Id, prov.Id, StringUtils.EscapeSql(this.Email)));
            }
        }

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
            try {
                safe = Safe.FromUserId(context, this.Id);
                safe.ClearKeys();
            } catch (Exception e) {
                //user has no safe yet
            }
            safe = new Safe(context);
            safe.OwnerId = this.Id;
            safe.GenerateKeys();
            safe.Store();
            return;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Recreates the safe.
        /// </summary>
        /// <param name="password">Password.</param>
//        public void RecreateSafe(string password) {
//            if (safe == null)
//                throw new Exception("User has no Safe");
//            safe.ClearKeys();
//            safe.GenerateKeys(password);
//            safe.Store();
//        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <returns>The public key.</returns>
        public string GetPublicKey() {
            if (!HasSafe())
                return null;
            return safe.GetBase64SSHPublicKey();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the private key.
        /// </summary>
        /// <returns>The private key.</returns>
        /// <param name="password">Password.</param>
        public string GetPrivateKey() {
            if (!HasSafe())
                return null;
            return safe.GetBase64SSHPrivateKey();
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
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP Distinguished Name
        /// </summary>
        /// <returns>The LDAP DN.</returns>
        private string CreateLdapDN(){
            string dn = string.Format("uid={0}, ou=people, dc=terradue, dc=com", this.Username);
            string dnn = Json2Ldap.NormalizeDN(dn);
            if (!Json2Ldap.IsValidDN(dnn)) throw new Exception(string.Format("Unvalid DN: {0}", dnn));
            return dnn;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP account.
        /// </summary>
        public void CreateLdapAccount(string password) {
            
            //open the connection
            Json2Ldap.Connect();

            string dn = CreateLdapDN();
            LdapUser ldapusr = this.ToLdapUser();
            ldapusr.DN = dn;
            ldapusr.Password = password;

            //login as ldap admin to have creation rights
            Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
            Json2Ldap.AddEntry(ldapusr);

            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP account.
        /// </summary>
        public void ChangeLdapPassword(string password) {

            //open the connection
            Json2Ldap.Connect();

            string dn = CreateLdapDN();

            Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
            Json2Ldap.ModifyPassword(dn, password);
            Json2Ldap.Close();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Deletes the LDAP account.
        /// </summary>
        public void DeleteLdapAccount(){
            //open the connection
            Json2Ldap.Connect();

            string dn = CreateLdapDN();

            //login as ldap admin to have creation rights
            Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
            Json2Ldap.DeleteEntry(dn);

            Json2Ldap.Close();
            
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Updates the LDAP account.
        /// </summary>
        public void UpdateLdapAccount(){

            //open the connection
            Json2Ldap.Connect();

            string dn = CreateLdapDN();
            LdapUser ldapusr = this.ToLdapUser();
            ldapusr.DN = dn;

            //login as ldap admin to have creation rights
            Json2Ldap.SimpleBind(context.GetConfigValue("ldap-admin-dn"), context.GetConfigValue("ldap-admin-pwd"));
            Json2Ldap.ModifyUserInformation(ldapusr);

            Json2Ldap.Close();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <returns>The plan.</returns>
        public string GetPlan() {
            return Plan.PlanToString(this.Plan.PlanType);
        }

    }
}

