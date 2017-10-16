﻿using System;
using Terradue.Portal;
using Terradue.OpenNebula;
using Terradue.Github;
using Terradue.Util;
using Terradue.Cloud;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Terradue.Ldap;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using Terradue.JFrog.Artifactory;
using System.Linq;

namespace Terradue.Corporate.Controller
{
    [EntityTable (null, EntityTableConfiguration.Custom, Storage = EntityTableStorage.Above)]
    public class UserT2 : User {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

        private Json2LdapFactory LdapFactory { get; set; }
        private PlanFactory PlanFactory { get; set; }
        private CatalogueFactory CatFactory { get; set; }
        private ArtifactoryFactory JFrogFactory { get; set; }

        private IUSER oneuser { get; set; }
        public IUSER OneUser {
            get {
                if (oneuser == null) {
                    oneuser = GetOneUser();
                }
                return oneuser;
            }
            set {

            }
        }

        /// <summary>
        /// True if user has a ldap account, else otherwise
        /// </summary>
        private bool hasldapaccount;

        /// <summary>
        /// Gets or sets the one password.
        /// </summary>
        /// <value>The one password.</value>
        public string OnePassword {
            get {
                if (this.IsPaying ()) {
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
            get {
                if (onegroups == null) {
                    onegroups = new List<string> ();
                    foreach (var group in OneUser.GROUPS) {
                        var gId = Int32.Parse (group);
                        if (gId > 0) onegroups.Add (oneClient.GroupGetInfo (gId).NAME);
                    }
                }
                return onegroups;
            }
        }

        public string OwnerDomainName {
            get {
                return this.Username + ".owner";
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



        /// <summary>
        /// Gets or sets the private domain of the user.
        /// </summary>
        /// <value>The domain.</value>
        public override Domain Domain {
            get {
                if (base.Domain == null) {
                    try {
                        base.Domain = Domain.FromIdentifier (context, Username);
                    } catch (Exception e) { }
                }
                return base.Domain;
            }
            set {
                base.Domain = value;
            }
        }

        public override int DomainId {
            get {
                if (Domain != null)
                    return Domain.Id;
                else return 0;
            }
            set {
                base.DomainId = value;
            }
        }

        public List<Plan> Plans { get; set; }

        [EntityDataField("links")]
        public string Links { get; set; }

        private List<string> linkslist;
        public List<string> LinksList { 
            get {
                if (linkslist == null) {
                    if (Links != null) linkslist = Links.Split(",".ToCharArray()).ToList();
                    else linkslist = new List<string>();
                }
                return linkslist;
            }
            set {
                if (value != null) {
                    linkslist = value;
                    Links = string.Join(",", linkslist);
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.Controller.UserT2"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public UserT2 (IfyContext context) : base (context)
        {
            OneCloudProvider oneCloud = (OneCloudProvider)CloudProvider.FromId (context, context.GetConfigIntegerValue ("One-default-provider"));
            this.oneClient = oneCloud.XmlRpc;
            this.LdapFactory = new Json2LdapFactory (context);
            this.Json2Ldap = LdapFactory.Json2Ldap;
            this.PlanFactory = new PlanFactory (context);
            this.CatFactory = new CatalogueFactory (context);
            this.JFrogFactory = new ArtifactoryFactory (context);

            this.Plans = PlanFactory.GetPlansForUser (this);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.Corporate.Controller.UserT2"/> class.
        /// </summary>
        /// <param name="user">User.</param>
        public UserT2 (IfyContext context, User user) : this (context)
        {
            this.Id = user.Id;
            this.Load ();

            this.Username = user.Username;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Email = user.Email;
            this.Affiliation = user.Affiliation;
            this.Country = user.Country;
            this.Level = user.Level;
        }

        //--------------------------------------------------------------------------------------------------------------

        public UserT2 (IfyContext context, LdapUser user) : this (context)
        {
            this.Username = user.Username;
            try {
                this.Load();
            }catch(Exception){}

            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Email = user.Email;
        }

        //--------------------------------------------------------------------------------------------------------------

        public override string AlternativeIdentifyingCondition {
            get {
                if (!string.IsNullOrEmpty (Username))
                    return String.Format ("t.username='{0}'", Username);
                else if (!string.IsNullOrEmpty (Email))
                    return String.Format ("t.email='{0}'", Email);
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
        public new static UserT2 FromId (IfyContext context, int id)
        {
            if (context.UserId == id) context.AccessLevel = EntityAccessLevel.Administrator;
            UserT2 user = new UserT2 (context);
            user.Id = id;
            user.Load ();
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new User instance representing the user with the specified unique name.
        /// </summary>
        /// <returns>The username.</returns>
        /// <param name="context">Context.</param>
        /// <param name="username">Username.</param>
        public new static UserT2 FromUsername (IfyContext context, string username)
        {
            UserT2 user = new UserT2 (context);
            user.Identifier = username;
            user.Load ();
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new User instance representing the user with the specified email.
        /// </summary>
        /// <returns>The email.</returns>
        /// <param name="context">Context.</param>
        /// <param name="email">Email.</param>
        public static UserT2 FromEmail (IfyContext context, string email)
        {
            UserT2 user = new UserT2 (context);
            user.Email = email;
            user.Load ();
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new User instance representing the user with the specified unique name or email.
        /// </summary>
        /// <returns>The username or email.</returns>
        /// <param name="context">Context.</param>
        /// <param name="username">Username.</param>
        public static UserT2 FromUsernameOrEmail (IfyContext context, string username)
        {
            UserT2 user;
            try {
                user = UserT2.FromUsername (context, username);
            } catch (Exception) {
                try {
                    user = UserT2.FromEmail (context, username);
                } catch (Exception) {
                    return null;
                }
            }
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Load this instance.
        /// </summary>
        public override void Load ()
        {
            base.Load ();

            //we only create domain for user having validated their email
            if (Domain == null && AccountStatus == AccountStatusType.Enabled) CreatePrivateDomain ();
        }

        public static UserT2 Create(IfyContext context, string username, string email, string password, AuthenticationType authType, int accountstatus, bool createLdap, string eosso = null, string originator = null, bool sendEmail = false){

            context.LogInfo(context, string.Format("Create new user: {0} - {1}", username, email));

            //test if email is already used, we do not create the new user
			try {
				UserT2.FromEmail(context, email);
				throw new EmailAlreadyUsedException("Your email is already associated to another account, we cannot create a new user");
			} catch (EmailAlreadyUsedException e) {
				throw e;
			} catch (Exception) { }

            //get unique username
            var exists = false;
			try {
				UserT2.FromUsername(context, username);
                exists = true;
			} catch (Exception) {}
			if (exists) {
                context.LogDebug(context, "Username alreasy in use, generating a new one");
                int i = 1;
	            while (exists && i < 100) {
			        var uname = string.Format("{0}{1}", username, i);
                    try{
                        UserT2.FromUsername(context, username);
                        i++;
                    } catch (Exception) {
                        exists = false;
                        username = uname;
                    }			        
			    }
                if (i == 99) throw new Exception("Sorry, we were not able to find a valid username");
                context.LogDebug(context, "Generated username: " + username);
			}
  
			//create user
			context.AccessLevel = EntityAccessLevel.Administrator;
			UserT2 usr = (UserT2)User.GetOrCreate(context, username, authType);
			usr.Email = email;
            usr.Level = UserLevel.User;
            usr.AccountStatus = accountstatus;
            usr.NeedsEmailConfirmation = false;
            usr.PasswordAuthenticationAllowed = true;
            if (!string.IsNullOrEmpty(originator)) usr.RegistrationOrigin = originator;
			usr.Store();
            context.LogDebug(context, "New user stored in DB");
			usr.LinkToAuthenticationProvider(authType, username);
            usr.CreateGithubProfile();

            if (createLdap) {
                context.LogDebug(context, "Creating LDAP account");
                usr.CreateLdapAccount(password);
                context.LogDebug(context, "Creating LDAP domain");
                usr.CreateLdapDomain();
                if (!string.IsNullOrEmpty(eosso)) {
                    context.LogDebug(context, "Adding Eosso attribute");
                    usr.EoSSO = eosso;
                    usr.UpdateLdapAccount();
                }
                context.LogDebug(context, "Generating apikey");
                usr.GenerateApiKey(password);
                context.LogDebug(context, "Creating Catalogue index");
                usr.CreateCatalogueIndex();//TODO: see if we need to use thread Tasks
                context.LogDebug(context, "Creating repository");
                usr.CreateRepository();
            }

            if (sendEmail) {
                try {
                    usr.SendMail(UserMailType.Registration, true);
                } catch (Exception) { }
            }

			try {
				var subject = "[T2 Portal] - User registration on Terradue Portal";
                var originatorText = !string.IsNullOrEmpty(originator) ? "\nThe request was performed from " + originator : "";
				var body = string.Format("This is an automatic email to notify that an account has been automatically created on Terradue Corporate Portal for the user {0} ({1}).{2}", usr.Username, usr.Email, originatorText);
				context.SendMail(context.GetConfigValue("SmtpUsername"), context.GetConfigValue("SmtpUsername"), subject, body);
			} catch (Exception) {
				//we dont want to send an error if mail was not sent
			}

            return usr;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the private domain.
        /// </summary>
        public void CreatePrivateDomain ()
        {
            //create new domain with Identifier = Username
            var privatedomain = new Domain (context);
            privatedomain.Identifier = Username;
            privatedomain.Name = Username;
            privatedomain.Description = "Domain of user " + Username;
            privatedomain.Kind = DomainKind.User;
            privatedomain.Store ();

            //set the userdomain
            Domain = privatedomain;

            //Get role owner
            var userRole = Role.FromIdentifier (context, "owner");

            //Grant role for user
            userRole.GrantToUser (this, Domain);

        }

        //--------------------------------------------------------------------------------------------------------------

        #region Authentication

        /// <summary>
        /// The authentication type.
        /// </summary>
        private List<AuthenticationType> authtypes;
        public List<AuthenticationType> AuthTypes {
            get {
                if (authtypes == null) {
                    var UserSession = System.Web.HttpContext.Current.Session ["user"] as UserInformation;
                    if (UserSession != null) authtypes = UserSession.AllAuthenticationTypes;
                }
                return authtypes;
            }
        }

        /// <summary>
        /// Check if user is authenticated using external provider
        /// </summary>
        /// <returns><c>true</c>, if external authentication was used, <c>false</c> otherwise.</returns>
        public bool IsExternalAuthentication () {
            if (AuthTypes == null) return false;
            foreach (var auth in AuthTypes)
                if (!(auth is Authentication.Ldap.LdapAuthenticationType) && auth.UsesExternalIdentityProvider) return true;

            return false;
        }

        /// <summary>
        /// Gets the external auth access token.
        /// </summary>
        /// <returns>The external auth access token.</returns>
        public string GetExternalAuthAccessToken () {
            if (AuthTypes == null) return null;
            foreach (var auth in AuthTypes) {
                if (auth is EverestAuthenticationType) {
                    return new EverestOauthClient (context).LoadTokenAccess ().Value;
                }
            }
            return null;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <returns>The token.</returns>
        public string GetToken ()
        {
            return base.ActivationToken;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether this instance is a paying user.
        /// </summary>
        /// <returns><c>true</c> if this instance is paying; otherwise, <c>false</c>.</returns>
        public bool IsPaying ()
        {
            return (this.DomainId != 0);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether this instance has github profile.
        /// </summary>
        /// <returns><c>true</c> if this instance has github profile; otherwise, <c>false</c>.</returns>
        public bool HasGithubProfile ()
        {
            try {
                GithubProfile.FromId (context, this.Id);
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        public override void Delete ()
        {
            //delete user on cloud
            if (OneUser != null) DeleteCloudAccount ();

            //delete user catalogue index
            //if (HasCatalogueIndex ()) {
            //    if (string.IsNullOrEmpty (ApiKey)) LoadApiKey ();
            //    CatFactory.DeleteIndex (this.Username, this.Username, this.ApiKey);
            //}

            //delete user on Artifactory
            //if (HasRepository ()) JFrogFactory.DeleteUser (this.Username);

            //delete user on LDAP
            if (LdapFactory.UserExists (Username)) DeleteLdapAccount ();

            //delete user on DB
            base.Delete ();
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Upgrade with the specified plan.
        /// </summary>
        /// <param name="plan">Plan.</param>
        public void Upgrade (Plan plan){
            if (plan.Domain == null) plan.Domain = Domain.FromIdentifier (context, "terradue");
            if (plan.Role == null) throw new Exception ("Invalid role for user upgrade");
            var role = this.GetRoleForDomain(plan.Domain);
            if (role != null && role.Id == plan.Role.Id){
                log.Debug(String.Format("Upgrade user {0} with role {1} for domain {2} - NOT NEEDED", this.Username, plan.Role.Name, plan.Domain.Name));
                return;
            }
            log.Info (String.Format ("Upgrade user {0} with role {1} for domain {2}", this.Username, plan.Role.Name, plan.Domain.Name));
            context.StartTransaction ();

            //sanity check
            if (!HasGithubProfile ()) CreateGithubProfile ();

            switch (plan.Role.Name) {
            case PlanFactory.NONE:
                break;
            case PlanFactory.TRIAL:
            case PlanFactory.EXPLORER:
            case PlanFactory.SCALER:
            case PlanFactory.PREMIUM:
                if (!HasCloudAccount ()) CreateCloudAccount ();
                if (!HasLdapDomain ()) CreateLdapDomain ();
                if (!HasCatalogueIndex()) CreateCatalogueIndex();
                if (!HasRepository()) CreateRepository();
                break;
            default:
                break;
            }

            //remove previous role of the user for this domain
            var roles = Role.GetUserRolesForDomain (context, this.Id, plan.Domain.Id);
            if (roles.Length > 0) {
                foreach (var userrole in roles)
                    userrole.RevokeFromUser (this, plan.Domain);
            }

            plan.Role.GrantToUser (this, plan.Domain);

            context.Commit ();
        }

        public Role GetRoleForDomain (string domainname)
        {
            var domain = Domain.FromIdentifier (context, domainname);
            return GetRoleForDomain (domain);
        }

        public Role GetRoleForDomain (Domain domain) {
            var roles = Role.GetUserRolesForDomain (context, this.Id, domain.Id);
            return roles.Length > 0 ? roles [0] : null;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Stores the password.
        /// </summary>
        /// <param name="pwd">Pwd.</param>
        public new void StorePassword (string pwd)
        {
            //password check
            ValidatePassword (pwd);
            base.StorePassword (pwd);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Validates the password.
        /// </summary>
        /// <param name="pwd">Pwd.</param>
        public static void ValidatePassword (string pwd)
        {
            if (pwd.Length < 8)
                throw new Exception ("Invalid password: You must use at least 8 characters");
            if (!Regex.Match (pwd, @"[A-Z]").Success)
                throw new Exception ("Invalid password: You must use at least one upper case value");
            if (!Regex.Match (pwd, @"[a-z]").Success)
                throw new Exception ("Invalid password: You must use at least one lower case value");
            if (!Regex.Match (pwd, @"[\d]").Success)
                throw new Exception ("Invalid password: You must use at least one numerical value");
            if (!Regex.Match (pwd, @"[!#@$%^&*()_+]").Success)
                throw new Exception ("Invalid password: You must use at least one special character");
            if (!Regex.Match (pwd, @"^[a-zA-Z0-9!#@$%^&*()_+]+$").Success)
                throw new Exception ("Invalid password: You password contains illegal characters");
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the github profile.
        /// </summary>
        public void CreateGithubProfile ()
        {
            GithubProfile github = new GithubProfile (context, this.Id);
            github.Store ();
        }

        //--------------------------------------------------------------------------------------------------------------

        #region ONE

        /// <summary>
        /// Gets the one user.
        /// </summary>
        /// <returns>The one user.</returns>
        public IUSER GetOneUser () {
            IUSER oneusr = null;
            if (oneId == 0) {
                try {
                    USER_POOL oneUsers = oneClient.UserGetPoolInfo ();
                    foreach (object item in oneUsers.Items) {
                        if (item is USER_POOLUSER) {
                            USER_POOLUSER oneUser = item as USER_POOLUSER;
                            if (oneUser.NAME == this.Username) {
                                oneId = Int32.Parse (oneUser.ID);
                                log.Debug (String.Format ("OneUser {0} found", oneId));
                                oneusr = oneUser;
                                break;
                            }
                        }
                    }
                } catch (Exception e) {
                    return null;
                }
            } else {
                USER oneUser = oneClient.UserGetInfo (oneId);
                oneusr = oneUser;
            }
            return oneusr;
        }

        /// <summary>
        /// Determines whether this instance has cloud account.
        /// </summary>
        /// <returns><c>true</c> if this instance has cloud account; otherwise, <c>false</c>.</returns>
        public bool HasCloudAccount ()
        {
            return context.GetQueryBooleanValue (String.Format ("SELECT username IS NOT NULL FROM usr_cloud WHERE id={0};", this.Id));
        }

        /// <summary>
        /// Creates the cloud profile.
        /// </summary>
        public void CreateCloudAccount ()
        {
            log.Info (String.Format ("Creating Cloud account for {0}", this.Username));
            if (this.Username.Equals (this.Email)) {
                log.Error (String.Format ("Username not set (equal to email)"));
                throw new Exception ("Please set a valid username before creating the Cloud account");
            }
            EntityList<CloudProvider> provs = new EntityList<CloudProvider> (context);
            provs.Load ();
            foreach (CloudProvider prov in provs) {
                log.Debug (String.Format ("Update usr_cloud table for provider {0}", prov.Id));
                context.Execute (String.Format ("DELETE FROM usr_cloud WHERE id={0} AND id_provider={1};", this.Id, prov.Id));
                context.Execute (String.Format ("INSERT IGNORE INTO usr_cloud (id, id_provider, username) VALUES ({0},{1},{2});", this.Id, prov.Id, StringUtils.EscapeSql (this.Username)));
            }

            if (OneUser == null) {

                //create user (using email as password)
                int id = oneClient.UserAllocate (this.Username, this.Email, "SSO");
            }
        }

        /// <summary>
        /// Updates the cloud account.
        /// </summary>
        /// <param name="plan">Plan.</param>
        public void UpdateCloudAccount (Plan plan)
        {
            if (OneUser != null) UpdateOneUser (OneUser, plan);
        }

        /// <summary>
        /// Updates the one user.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="plan">Plan.</param>
        private void UpdateOneUser (object user, Plan plan)
        {
            var views = "";
            var defaultview = "";
            switch (plan.Role.Name) {
            case PlanFactory.EXPLORER:
                views = PlanFactory.EXPLORER;
                defaultview = PlanFactory.EXPLORER;
                break;
            case PlanFactory.SCALER:
                views = PlanFactory.SCALER;
                defaultview = PlanFactory.SCALER;
                break;
            case PlanFactory.PREMIUM:
                views = PlanFactory.SCALER + "," + PlanFactory.PREMIUM;
                defaultview = PlanFactory.PREMIUM;
                break;
            default:
                break;
            }

            List<KeyValuePair<string, string>> templatePairs = new List<KeyValuePair<string, string>> ();
            templatePairs.Add (new KeyValuePair<string, string> ("USERNAME", this.Username));
            templatePairs.Add (new KeyValuePair<string, string> ("SUNSTONE_VIEWS", views));
            templatePairs.Add (new KeyValuePair<string, string> ("DEFAULT_VIEW", defaultview));

            XmlNode [] template = null;
            int id = 0;

            if (user is USER) {
                template = (XmlNode [])((user as USER).TEMPLATE);
                id = Int32.Parse ((user as USER).ID);
            } else if (user is USER_POOLUSER) {
                template = (XmlNode [])((user as USER_POOLUSER).TEMPLATE);
                id = Int32.Parse ((user as USER_POOLUSER).ID);
            }

            if (template != null && id != 0) {
                //update user template
                string templateUser = CreateTemplate (template, templatePairs);
                if (!oneClient.UserUpdate (id, templateUser)) throw new Exception ("Error during update of user");
            }
        }

        /// <summary>
        /// Updates the one group.
        /// </summary>
        /// <param name="grpId">Group identifier.</param>
        public void UpdateOneGroup (int grpId)
        {
            if (OneUser != null) oneClient.UserUpdateGroup (Int32.Parse (OneUser.ID), grpId);
        }

        /// <summary>
        /// Creates the template.
        /// </summary>
        /// <returns>The template.</returns>
        /// <param name="template">Template.</param>
        /// <param name="pairs">Pairs.</param>
        private string CreateTemplate (XmlNode [] template, List<KeyValuePair<string, string>> pairs)
        {
            List<KeyValuePair<string, string>> originalTemplate = new List<KeyValuePair<string, string>> ();
            List<KeyValuePair<string, string>> resultTemplate = new List<KeyValuePair<string, string>> ();
            for (int i = 0; i < template.Length; i++) {
                originalTemplate.Add (new KeyValuePair<string, string> (template [i].Name, template [i].InnerText));
            }

            foreach (KeyValuePair<string, string> original in originalTemplate) {
                bool exists = false;
                foreach (KeyValuePair<string, string> pair in pairs) {
                    if (original.Key.Equals (pair.Key)) {
                        exists = true;
                        break;
                    }
                }
                if (!exists) pairs.Add (original);
            }

            string templateUser = "<TEMPLATE>";
            foreach (KeyValuePair<string, string> pair in pairs) {
                templateUser += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
            }
            templateUser += "</TEMPLATE>";
            return templateUser;
        }

        /// <summary>
        /// Deletes the cloud account.
        /// </summary>
        public void DeleteCloudAccount ()
        {
            if (OneUser != null) oneClient.UserDelete (Int32.Parse (OneUser.ID));
            context.Execute (String.Format ("DELETE FROM usr_cloud WHERE id={0} AND id_provider={1};", this.Id, context.GetConfigIntegerValue ("One-default-provider")));
        }

        #endregion

       

        #region SAFE

        /// <summary>
        /// Creates the safe.
        /// </summary>
        public void CreateSafe ()
        {
            Safe safe = new Safe (context);
            safe.GenerateKeys ();
            this.PublicKey = safe.GetBase64SSHPublicKey ();
            this.PrivateKey = safe.GetBase64SSHPrivateKeyOpenSSL ();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region LDAP

        /// <summary>
        /// To LDAP user.
        /// </summary>
        /// <returns>The LDAP user.</returns>
        public LdapUser ToLdapUser ()
        {
            LdapUser user = new LdapUser ();
            user.Username = this.Username;
            user.Email = this.Email;
            user.FirstName = this.FirstName;
            user.LastName = this.LastName;
            user.Name = string.Format ("{0} {1}", this.FirstName, this.LastName);
            user.PublicKey = this.PublicKey;
            user.EoSSO = this.EoSSO;
            user.ApiKey = this.ApiKey;
            return user;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Check if user has ldap account
        /// </summary>
        /// <returns><c>true</c>, if LDAP account exists, <c>false</c> otherwise.</returns>
        public bool HasLdapAccount ()
        {
            if (!hasldapaccount) hasldapaccount = LdapFactory.UserExists (this.Username);
            return hasldapaccount;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP Distinguished Name for people
        /// </summary>
        /// <returns>The LDAP DN.</returns>
        private string CreateLdapDNforPeople ()
        {
            return LdapFactory.CreateLdapPeopleDN (this.Username);
        }

        /// <summary>
        /// /// Creates the LDAP Distinguished Name for domain.
        /// </summary>
        /// <returns>The LDAP DN for domain.</returns>
        private string CreateLdapDNforDomain ()
        {
            return LdapFactory.CreateLdapDNforDomain (OwnerDomainName, this.Username);
        }

        /// <summary>
        /// /// Creates the LDAP Distinguished Name for domain level1.
        /// </summary>
        /// <returns>The LDAP DN for domain level1.</returns>
        private string CreateLdapDNforDomainLevel1 ()
        {
            return LdapFactory.CreateLdapDNforDomain (null, this.Username);
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP account.
        /// </summary>
        public void CreateLdapAccount (string password)
        {

            //open the connection
            Json2Ldap.Connect ();
            try {

                string dn = CreateLdapDNforPeople ();
                LdapUser ldapusr = this.ToLdapUser ();
                ldapusr.DN = dn;
                ldapusr.Password = GenerateSaltedSHA1 (password);
                ldapusr.ApiKey = this.ApiKey;

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));
                Json2Ldap.AddEntry (ldapusr);
                //                Json2Ldap.ModifyPassword(dn, password, password);

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();
            hasldapaccount = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates the LDAP account.
        /// </summary>
        public void ChangeLdapPassword (string newpassword, string oldpassword = null, bool admin = false)
        {

            //open the connection
            Json2Ldap.Connect ();
            try {

                string dn = CreateLdapDNforPeople ();

                if (admin) Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));
                else Json2Ldap.SimpleBind (dn, oldpassword);
                Json2Ldap.ModifyPassword (dn, newpassword, oldpassword);
            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();
            hasldapaccount = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Deletes the LDAP account.
        /// </summary>
        public void DeleteLdapAccount ()
        {
            //open the connection
            Json2Ldap.Connect ();
            try {

                string dn = CreateLdapDNforPeople ();

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));
                Json2Ldap.DeleteEntry (dn);

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();
            hasldapaccount = false;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Updates the username.
        /// </summary>
        /// <param name="oldUid">Old uid.</param>
        /// <param name="newUid">New uid.</param>
        public void UpdateUsername (string oldUid, string newUid){
            
            ValidateUsername (newUid);

            //change username on LDAP
            var dn = LdapFactory.CreateLdapPeopleDN (oldUid);

            //open the connection
            Json2Ldap.Connect ();
            try {

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));

                Json2Ldap.ModifyUID (dn, newUid);

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            //Change username on the db
            AuthenticationType authType = IfyWebContext.GetAuthenticationType (typeof (Terradue.Authentication.Ldap.LdapAuthenticationType));
            string sql = string.Format ("UPDATE usr_auth SET username={0} WHERE id_usr={1} and id_auth={2};",
                                       StringUtils.EscapeSql (newUid),
                                       this.Id,
                                       authType.Id);
            context.Execute (sql);
            sql = string.Format ("UPDATE usr SET username={0} WHERE id={1};",
                                StringUtils.EscapeSql (newUid),
                                this.Id);
            context.Execute (sql);

            this.Username = newUid;

            hasldapaccount = true;
        }

        /// <summary>
        /// Updates the LDAP account.
        /// </summary>
        /// <param name="password">Password.</param>
        public void UpdateLdapAccount (string password = null)
        {

            //open the connection
            Json2Ldap.Connect ();

            try {

                string dn = CreateLdapDNforPeople ();

                LdapUser ldapusr = this.ToLdapUser ();
                ldapusr.DN = dn;

                //simple bind to have creation rights
                if (password == null) {
                    Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));
                } else {
                    Json2Ldap.SimpleBind (dn, password);
                }

                try {
                    Json2Ldap.ModifyUserInformation (ldapusr);
                } catch (Exception e) {
                    try {
                        //user may not have sshPublicKey | eossoUserid | apiKey
                        if (e.Message.Contains ("sshPublicKey") || e.Message.Contains ("sshUsername") || e.Message.Contains ("apiKey")) {
                            Json2Ldap.AddNewAttributeString (dn, "objectClass", "ldapPublicKey");
                            Json2Ldap.ModifyUserInformation (ldapusr);
                        } else if (e.Message.Contains ("eossoUserid")) {
                            Json2Ldap.AddNewAttributeString (dn, "objectClass", "eossoAccount");
                            Json2Ldap.ModifyUserInformation (ldapusr);
                        }
                        throw e;
                    } catch (Exception e2) {
                        throw e2;
                    }
                }

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            hasldapaccount = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Validates the username.
        /// </summary>
        /// <param name="username">Username.</param>
        public static void ValidateUsername (string username)
        {
            if (Regex.IsMatch (username, "^[a-z_][a-z0-9_-]{1,30}[$]?$"))
                return;

            if (username.Length > 32)
                throw new Exception ("Invalid Cloud username: It must have a maximum of 32 characters");
            if (!Regex.IsMatch (username, "^[a-z_]"))
                throw new Exception ("Invalid Cloud username: It must begin with a lower case letter or an underscore");
            throw new Exception ("Invalid Cloud username: You must use only lower case letters, digits, underscores, or dashes");
        }

        /// <summary>
        /// Makes the username valid.
        /// </summary>
        /// <returns>The username valid.</returns>
        /// <param name="username">Username.</param>
        public static string MakeUsernameValid (string username)
        {
            if (string.IsNullOrEmpty (username)) throw new Exception ("empty username");

            var result = username.ToLower ().Replace (" ", "").Replace (".", "").Replace ("-", "").Replace ("_", "");
            ValidateUsername (result);
            return result;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Deletes the public key.
        /// </summary>
        public void DeletePublicKey ()
        {
            //open the connection
            Json2Ldap.Connect ();
            try {

                string dn = CreateLdapDNforPeople ();

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));
                Json2Ldap.DeleteAttributeString (dn, "sshPublicKey", null);

                //TODO: delete also from OpenNebula

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            hasldapaccount = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Loads the LDAP info.
        /// </summary>
        public void LoadLdapInfo (string password = null)
        {
            Json2Ldap.Connect ();
            try {
                var dn = CreateLdapDNforPeople ();
                if (password != null) Json2Ldap.SimpleBind (dn, password);
                var ldapusr = this.Json2Ldap.GetEntry (dn);
                if (ldapusr != null) {
                    if (!string.IsNullOrEmpty (ldapusr.Username)) this.Username = ldapusr.Username;
                    if (!string.IsNullOrEmpty (ldapusr.Email)) this.Email = ldapusr.Email;
                    if (!string.IsNullOrEmpty (ldapusr.FirstName)) this.FirstName = ldapusr.FirstName;
                    if (!string.IsNullOrEmpty (ldapusr.LastName)) this.LastName = ldapusr.LastName;
                    if (!string.IsNullOrEmpty (ldapusr.PublicKey)) this.PublicKey = ldapusr.PublicKey;
                    if (!string.IsNullOrEmpty (ldapusr.ApiKey)) this.ApiKey = ldapusr.ApiKey;
                }
            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            hasldapaccount = true;
        }

        /// <summary>
        /// Publics the load ssh pub key.
        /// </summary>
        /// <param name="username">Username.</param>
        public void PublicLoadSshPubKey (string username)
        {
            Json2Ldap.Connect ();
            try {
                var dn = LdapFactory.CreateLdapPeopleDN (username);
                var ldapusr = this.Json2Ldap.GetEntry (dn);
                if (ldapusr != null) {
                    if (!string.IsNullOrEmpty (ldapusr.PublicKey)) this.PublicKey = ldapusr.PublicKey;
                }
            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            hasldapaccount = true;
        }

        /// <summary>
        /// Determines whether this instance has LDAP domain.
        /// </summary>
        /// <returns><c>true</c> if this instance has LDAP domain; otherwise, <c>false</c>.</returns>
        public bool HasLdapDomain ()
        {
            Json2Ldap.Connect ();
            bool result = false;
            try {
                var dn = CreateLdapDNforDomain ();
                result = this.Json2Ldap.GetEntry (dn) != null;
            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();
            hasldapaccount = true;
            return result;
        }

        /// <summary>
        /// Creates the LDAP domain.
        /// </summary>
        public void CreateLdapDomain ()
        {

            //open the connection
            Json2Ldap.Connect ();
            try {

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));

                //create first level entry
                ParamsAttributes attributes = new ParamsAttributes ();
                attributes.objectClass = new List<string> { "organizationalUnit", "top" };
                attributes.ou = this.Username;

                Json2Ldap.AddEntry (CreateLdapDNforDomainLevel1 (), attributes);

                //create second level entry
                attributes = new ParamsAttributes ();
                attributes.objectClass = new List<string> { "groupOfUniqueNames" };
                attributes.cn = OwnerDomainName;
                attributes.uniqueMember = CreateLdapDNforPeople ();
                attributes.description = "Owner of user domain " + this.Username;

                Json2Ldap.AddEntry (CreateLdapDNforDomain (), attributes);

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            //Add group on Artifactory
            CreateArtifactoryGroup ();

            hasldapaccount = true;
        }

        /// <summary>
        /// Adds to LDAP domain.
        /// </summary>
        /// <param name="subdomain">Subdomain.</param>
        /// <param name="parentDomain">Parent domain.</param>
        public void AddToLdapDomain (string subdomain, string parentDomain) {
            Json2Ldap.Connect ();

            try {
                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));

                var dnD = LdapFactory.CreateLdapDNforDomain (subdomain, parentDomain);
                var dnP = CreateLdapDNforPeople ();

                Json2Ldap.ModifyEntry (dnD, "uniqueMember", new List<string> { dnP }, "add");
            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();
        }


        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Add the public key.
        /// </summary>
        public void AddPublicKeyAttribute ()
        {
            Json2Ldap.Connect ();
            try {

                Json2Ldap.AddNewAttributeString (CreateLdapDNforPeople (), "objectClass", "ldapPublicKey");

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            hasldapaccount = true;
        }

        public static string GenerateSaltedSHA1 (string plainTextString)
        {
            HashAlgorithm algorithm = new SHA1Managed ();
            var saltBytes = GenerateSalt (4);
            var plainTextBytes = Encoding.ASCII.GetBytes (plainTextString);

            var plainTextWithSaltBytes = AppendByteArray (plainTextBytes, saltBytes);
            var saltedSHA1Bytes = algorithm.ComputeHash (plainTextWithSaltBytes);
            var saltedSHA1WithAppendedSaltBytes = AppendByteArray (saltedSHA1Bytes, saltBytes);

            return "{SSHA}" + Convert.ToBase64String (saltedSHA1WithAppendedSaltBytes);
        }


        private static byte [] GenerateSalt (int saltSize)
        {
            var rng = new RNGCryptoServiceProvider ();
            var buff = new byte [saltSize];
            rng.GetBytes (buff);
            return buff;
        }

        private static byte [] AppendByteArray (byte [] byteArray1, byte [] byteArray2)
        {
            var byteArrayResult =
                new byte [byteArray1.Length + byteArray2.Length];

            for (var i = 0; i < byteArray1.Length; i++)
                byteArrayResult [i] = byteArray1 [i];
            for (var i = 0; i < byteArray2.Length; i++)
                byteArrayResult [byteArray1.Length + i] = byteArray2 [i];

            return byteArrayResult;
        }

        #endregion

        #region APIKEY
        /// <summary>
        /// Loads the API key.
        /// </summary>
        /// <param name="password">Password.</param>
        public void LoadApiKey (string password = null)
        {
            Json2Ldap.Connect ();
            try {
                var dn = CreateLdapDNforPeople ();
                if (password != null) Json2Ldap.SimpleBind (dn, password);
                else Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));
                var ldapusr = this.Json2Ldap.GetEntryAttribute (dn, "apiKey");
                if (ldapusr != null) {
                    this.ApiKey = ldapusr.ApiKey;
                }
            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            hasldapaccount = true;
        }

        /// <summary>
        /// Saves the API key on LDAP.
        /// </summary>
        /// <param name="password">Password.</param>
        public void SaveApiKeyOnLDAP (string password)
        {
            Json2Ldap.Connect ();
            try {
                var dn = CreateLdapDNforPeople ();
                Json2Ldap.SimpleBind (dn, password);

                try {
                    Json2Ldap.ModifyUserAttribute (dn, "apiKey", this.ApiKey);
                } catch (Exception e) {
                    try {
                        //user may not have sshPublicKey | eossoUserid | apiKey
                        if (e.Message.Contains ("sshPublicKey") || e.Message.Contains ("sshUsername") || e.Message.Contains ("apiKey")) {
                            Json2Ldap.AddNewAttributeString (dn, "objectClass", "ldapPublicKey");
                            Json2Ldap.ModifyUserAttribute (dn, "apiKey", this.ApiKey);
                        } else throw e;
                    } catch (Exception e2) {
                        throw e2;
                    }
                }

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            hasldapaccount = true;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Generates the API key.
        /// </summary>
        /// <param name="password">Password.</param>
        public void GenerateApiKey (string password)
        {

            //Api Key is saved also on Artifactory
            this.ApiKey = this.JFrogFactory.CreateApiKey (this.Username, password);

            //save on LDAP
            this.SaveApiKeyOnLDAP (password);
        }

        /// <summary>
        /// Revokes the API key.
        /// </summary>
        public void RevokeApiKey (string password)
        {

            //open the connection
            Json2Ldap.Connect ();
            try {

                string dn = CreateLdapDNforPeople ();

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind (context.GetConfigValue ("ldap-admin-dn"), context.GetConfigValue ("ldap-admin-pwd"));
                Json2Ldap.DeleteAttributeString (dn, "apiKey", null);
                this.ApiKey = null;

            } catch (Exception e) {
                Json2Ldap.Close ();
                throw e;
            }
            Json2Ldap.Close ();

            //revoke api key also from Artifactory
            JFrogFactory.RevokeApiKey (this.Username, password);

            hasldapaccount = true;
        }

        #endregion

        #region Catalogue

        public bool HasCatalogueIndex ()
        {
            try {
                if (this.ApiKey == null) LoadApiKey ();

                return this.CatFactory.IndexExists (this.Username, this.Username, this.ApiKey);
            } catch (Exception e) { return false; }
        }

        public void CreateCatalogueIndex ()
        {
            CreateCatalogueIndex (this.Username);
        }

        public void CreateCatalogueIndex (string index)
        {
            if (this.ApiKey == null) LoadApiKey ();
            this.CatFactory.CreateIndex (index, this.Username, this.ApiKey);
        }

        public List<string> GetUserCatalogueIndexes ()
        {
            var result = new List<string> ();

            if (this.HasCatalogueIndex ())
                result.Add (this.CatFactory.GetUserIndexUrl (this.Username));

            return result;
        }

        #endregion

        #region Artifactory

        /// <summary>
        /// Determines whether this instance has repository.
        /// </summary>
        /// <returns><c>true</c> if this instance has repository; otherwise, <c>false</c>.</returns>
        public bool HasRepository ()
        {
            return this.JFrogFactory.RepositoryExists (this.Username);
        }

        /// <summary>
        /// Creates the repository.
        /// </summary>
        public void CreateRepository ()
        {
            this.CreateRepository (this.Username);
        }

        /// <summary>
        /// Creates the repository.
        /// </summary>
        /// <param name="repo">Repo.</param>
        public void CreateRepository (string repo)
        {
            if (repo == null) repo = this.Username;

            this.JFrogFactory.CreateLocalRepository (repo);

            //sanity check: does user has domain on ldap ?
            if (!HasLdapDomain ()) CreateLdapDomain ();

            //Create permission for groups on new repo
            this.JFrogFactory.CreatePermissionForGroupOnRepo (OwnerDomainName, repo);
        }

        /// <summary>
        /// Gets the user repositories.
        /// </summary>
        /// <returns>The user repositories.</returns>
        public List<RepositoriesSummary> GetUserRepositories ()
        {
            var result = new List<RepositoriesSummary> ();
            if (this.HasRepository ())
                result.Add (JFrogFactory.GetStorageInfo (this.Username));
            return result;

        }

        /// <summary>
        /// Syncs the artifactory.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void SyncArtifactory (string username, string password)
        {
            if (this.AccountStatus == AccountStatusType.Enabled && this.Username != this.Email) {
                try {
                    JFrogFactory.Sync (username, password);
                } catch (Exception e) {
                    log.ErrorFormat (e.Message + " - " + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Creates the artifactory group.
        /// </summary>
        public void CreateArtifactoryGroup ()
        {
            //Add group on Artifactory
            JFrogFactory.CreateGroup (OwnerDomainName, CreateLdapDNforDomain ());
        }

        /// <summary>
        /// Gets the user artifactory groups.
        /// </summary>
        /// <returns>The user artifactory groups.</returns>
        public List<string> GetUserArtifactoryGroups ()
        {
            return JFrogFactory.GetGroupsForUser (this.Username);
        }

        /// <summary>
        /// Determines whether this instance has owner group.
        /// </summary>
        /// <returns><c>true</c> if this instance has owner group; otherwise, <c>false</c>.</returns>
        public bool HasOwnerGroup ()
        {
            try {
                foreach (string g in GetUserArtifactoryGroups ()) {
                    if (g.Equals (OwnerDomainName)) return true;
                }
            } catch (Exception e) { return false; }
            return false;
        }

        /// <summary>
        /// Owners the group exists.
        /// </summary>
        /// <returns><c>true</c>, if group exists was ownered, <c>false</c> otherwise.</returns>
        public bool OwnerGroupExists ()
        {
            try {
                foreach (string g in JFrogFactory.GetGroups ()) {
                    if (g.Equals (OwnerDomainName)) return true;
                }
            } catch (Exception e) { return false; }
            return false;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        public void ValidateNewEmail (string email)
        {

            //simple checks on the email
            if (string.IsNullOrEmpty (email)) throw new Exception ("Your new email is empty.");
            if (!email.Contains ("@")) throw new Exception ("Invalid email.");

            //check the email is not already used on LDAP
            if (LdapFactory.GetUserFromEmail (email) != null) throw new Exception ("This email is already used.");
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the first login date.
        /// </summary>
        /// <returns>The first login date.</returns>
        public DateTime GetFirstLoginDate ()
        {
            DateTime value = DateTime.MinValue;
            try {
                System.Data.IDbConnection dbConnection = context.GetDbConnection ();
                string sql = String.Format ("SELECT log_time FROM usrsession WHERE id_usr={0} ORDER BY log_time ASC LIMIT 1;", this.Id);
                System.Data.IDataReader reader = context.GetQueryResult (sql, dbConnection);
                if (reader.Read ()) value = context.GetDateTimeValue (reader, 0);
                context.CloseQueryResult (reader, dbConnection);
            } catch (Exception) { }
            return value;
        }

        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the last login date.
        /// </summary>
        /// <returns>The last login date.</returns>
        public DateTime GetLastLoginDate ()
        {
            DateTime value = DateTime.MinValue;
            try {
                System.Data.IDbConnection dbConnection = context.GetDbConnection ();
                string sql = String.Format ("SELECT log_time FROM usrsession WHERE id_usr={0} ORDER BY log_time DESC LIMIT 1;", this.Id);
                System.Data.IDataReader reader = context.GetQueryResult (sql, dbConnection);
                if (reader.Read ()) value = context.GetDateTimeValue (reader, 0);
                context.CloseQueryResult (reader, dbConnection);
            } catch (Exception) { }
            return value;
        }

    }
}

