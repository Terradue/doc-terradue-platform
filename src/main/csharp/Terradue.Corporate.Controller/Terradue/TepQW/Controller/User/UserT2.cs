using System;
using Terradue.Portal;
using Terradue.OpenNebula;
using Terradue.Github;
using Terradue.Util;

namespace Terradue.Corporate.Controller {
    [EntityTable(null, EntityTableConfiguration.Custom, Storage = EntityTableStorage.Above)]
    public class UserT2 : User {

        #region T2Certificate

        /// <summary>
        /// Gets the cert subject.
        /// </summary>
        /// <value>The cert subject.</value>
        private string certsubject { get; set; }
        public string CertSubject {
            get {
                if (certsubject == null) {
                    try {
                        Terradue.Security.Certification.CertificateUser certUser = Terradue.Security.Certification.CertificateUser.FromId(context, Id);
                        certsubject = certUser.CertificateSubject;
                    } catch (EntityNotFoundException e) {
                        certsubject = null;
                    }
                }
                return certsubject;
            }
        }

        /// <summary>
        /// Gets the x509 certificate.
        /// </summary>
        /// <value>The x509 certificate.</value>
        public System.Security.Cryptography.X509Certificates.X509Certificate2 X509Certificate {
            get {
                try {
                    Terradue.Security.Certification.CertificateUser certUser = Terradue.Security.Certification.CertificateUser.FromId(context,Id);
                    return certUser.X509Certificate;
                }
                catch ( EntityNotFoundException e ){
                    return null;
                }
            }
        }

        #endregion

        #region OpenNebula

        /// <summary>
        /// Gets or sets the one password.
        /// </summary>
        /// <value>The one password.</value>
        public string OnePassword { 
            get{ 
                if (onepwd == null) {
                    if (oneId == 0) {
                        try{
                            USER_POOL oneUsers = oneClient.UserGetPoolInfo();
                            foreach (object item in oneUsers.Items) {
                                if (item is USER_POOLUSER) {
                                    USER_POOLUSER oneUser = item as USER_POOLUSER;
                                    if (oneUser.NAME == this.Username) {
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
            oneClient = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
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

        public override void Store(){
            bool isnew = (this.Id == 0);
            base.Store();
            if (isnew) {
                //create github profile
                GithubProfile github = new GithubProfile(context, this.Id);
                github.Store();
                //create certificate record
                context.Execute(String.Format("INSERT INTO usrcert (id_usr) VALUES ({0});", this.Id));
            }
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

        public new static UserT2 FromUsername(IfyContext context, string username){
            UserT2 user = new UserT2(context);
            user.Identifier = username;
            user.Load();
            return user;
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

