using System;
using Terradue.Portal;
using Terradue.OpenNebula;
using Terradue.Github;
using Terradue.Util;
using Terradue.Cloud;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Terradue.Crowd;

namespace Terradue.Corporate.Controller {
    [EntityTable(null, EntityTableConfiguration.Custom, Storage = EntityTableStorage.Above)]
    public class UserT2 : User {
        
        #region OpenNebula

        /// <summary>
        /// Gets or sets the one password.
        /// </summary>
        /// <value>The one password.</value>
        public string OnePassword { 
            get{ 
                if (onepwd == null && this.IsPaying()) {
                    if (oneId == 0) {
                        try{
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
                        }catch(Exception e){
                            return null;
                        }
                    } else {
                        USER oneUser = oneClient.UserGetInfo(oneId);
                        onepwd = oneUser.PASSWORD;
                    }
                }
                return onepwd;
            } 
            set{
                onepwd = value;
            } 
        }
        private string onepwd { get; set; }
        private int oneId { get; set; }
        private OneClient oneClient { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.Controller.UserT2"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public UserT2(IfyContext context) : base(context) {
            OneCloudProvider oneCloud = (OneCloudProvider)CloudProvider.FromId(context, context.GetConfigIntegerValue("One-default-provider"));
            oneClient = oneCloud.XmlRpc;
        }

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

        /// <summary>
        /// Creates a new User instance representing the user with the specified ID.
        /// </summary>
        /// <returns>The identifier.</returns>
        /// <param name="context">Context.</param>
        /// <param name="id">Identifier.</param>
        public new static UserT2 FromId(IfyContext context, int id){
            UserT2 user = new UserT2(context);
            user.Id = id;
            user.Load();
            return user;
        }

        /// <summary>
        /// Creates a new User instance representing the user with the specified unique name.
        /// </summary>
        /// <returns>The username.</returns>
        /// <param name="context">Context.</param>
        /// <param name="username">Username.</param>
        public new static UserT2 FromUsername(IfyContext context, string username){
            UserT2 user = new UserT2(context);
            user.Identifier = username;
            user.Load();
            return user;
        }

        /// <summary>
        /// To CrowdUser type.
        /// </summary>
        /// <returns>The crowd user.</returns>
        public CrowdUser ToCrowdUser(){
            CrowdUser cuser = new CrowdUser();
            cuser.name = this.Username;
            cuser.display_name = this.Username;
            cuser.first_name = this.FirstName;
            cuser.last_name = this.LastName;
            cuser.email = this.Email;
            cuser.active = true;
            cuser.password = this.GetUserPassword();

            return cuser;
        }

        /// <summary>
        /// Upgrade the account of the current user
        /// </summary>
        /// <param name="level">Level.</param>
        public void Upgrade(int level){
            context.StartTransaction();
            CreateGithubProfile();
            CreateCloudProfile();
            CreateDomain();
            CreateSafe();
            try{
                CreateLdapAccount();
            }catch(Exception e){
                //TODO: get already existing exception and if other do ClearSafe()
                throw e;
            }
            context.Commit();
        }

        /// <summary>
        /// Determines whether this instance is a paying user.
        /// </summary>
        /// <returns><c>true</c> if this instance is paying; otherwise, <c>false</c>.</returns>
        public bool IsPaying(){
            return context.GetQueryBooleanValue(String.Format("SELECT id_domain IS NOT NULL FROM usr WHERE id={0};", this.Id));
        }

        /// <summary>
        /// Writes the item to the database.
        /// </summary>
        public override void Store(){
            bool isnew = (this.Id == 0);
            base.Store();
            if (isnew && IsPaying()) {
                CreateGithubProfile();
                CreateCloudProfile();
            }
        }

        private string GetUserPassword(){
            //decrypter ds l'autre sens
//            return context.GetQueryBooleanValue(String.Format("SELECT id_domain IS NOT NULL FROM usr WHERE id={0};", this.Id));
            return "";
        }

        /// <summary>
        /// Stores the password.
        /// </summary>
        /// <param name="pwd">Pwd.</param>
        public new void StorePassword(string pwd){
            //password check
            ValidatePassword(pwd);
            base.StorePassword(pwd);
        }

        /// <summary>
        /// Validates the password.
        /// </summary>
        /// <param name="pwd">Pwd.</param>
        public void ValidatePassword(string pwd){
            if (pwd.Length < 8) throw new Exception("Invalid password: You must use at least 8 characters");
            if (!Regex.Match(pwd, @"[A-Z]").Success) throw new Exception("Invalid password: You must use at least one upper case value");
            if (!Regex.Match(pwd, @"[a-z]").Success) throw new Exception("Invalid password: You must use at least one lower case value");
            if (!Regex.Match(pwd, @"[\d]").Success) throw new Exception("Invalid password: You must use at least one numerical value");
            if (!Regex.Match(pwd, @"[!#@$%^&*()_+]").Success) throw new Exception("Invalid password: You must use at least one special character");
            if (!Regex.Match(pwd, @"^[a-zA-Z0-9!#@$%^&*()_+]+$").Success) throw new Exception("Invalid password: You password contains illegal characters");
        }

        /// <summary>
        /// Creates the github profile.
        /// </summary>
        protected void CreateGithubProfile(){
            GithubProfile github = new GithubProfile(context, this.Id);
            github.Store();
        }

        /// <summary>
        /// Creates the cloud profile.
        /// </summary>
        protected void CreateCloudProfile(){
            EntityList<CloudProvider> provs = new EntityList<CloudProvider>(context);
            provs.Load();
            foreach (CloudProvider prov in provs) {
                context.Execute(String.Format("INSERT IGNORE INTO usr_cloud (id, id_provider, username) VALUES ({0},{1},{2});", this.Id, prov.Id, StringUtils.EscapeSql(this.Email)));
            }
        }

        /// <summary>
        /// Creates the domain.
        /// </summary>
        protected void CreateDomain(){
            Domain domain = new Domain(context);
            domain.Identifier = this.Username;
            domain.Name = this.Username;
            domain.Description = string.Format("Domain belonging to user {0}",this.Username);
            domain.Store();
            this.DomainId = domain.Id;
            this.Store();
        }

        /// <summary>
        /// Creates the safe.
        /// </summary>
        protected void CreateSafe(){
            Safe safe = new Safe(context);
            safe.OwnerId = this.Id;
            string password = "";
            safe.GenerateKeys(password);
            safe.Store();
        }

        /// <summary>
        /// Creates the LDAP account.
        /// </summary>
        protected void CreateLdapAccount(){
            CrowdClient client = new CrowdClient(context.GetConfigValue("Crowd-api-url"), context.GetConfigValue("Crowd-app-name"), context.GetConfigValue("Crowd-app-pwd"));
            client.CreateUser(this.ToCrowdUser());
        }

        /// <summary>
        /// Removes the certificate.
        /// </summary>
        public void RemoveCertificate() {
            string sql = String.Format("UPDATE usrcert SET cert_subject=NULL, cert_content_pem=NULL WHERE id_usr={0};",this.Id);
            context.Execute(sql);
        }

    }
}

