using System;
using Terradue.Portal;
using Terradue.OpenNebula;

namespace Terradue.Corporate.Controller {
    [EntityTable(null, EntityTableConfiguration.Custom, Storage = EntityTableStorage.Above)]
    [EntityReferenceTable("usr_github", USERGIT_TABLE, ReferenceField = "id", IdField = "id_usr")]
    public class UserT2 : User {

        private const int USERGIT_TABLE = 1;

        /// <summary>
        /// Gets or sets the name of the github.
        /// </summary>
        /// <value>The name of the github.</value>
        [EntityForeignField("username", USERGIT_TABLE)]
        public string GithubName { get; set; }

        /// <summary>
        /// Gets the cert subject.
        /// </summary>
        /// <value>The cert subject.</value>
        public string CertSubject {
            get {
                try {
                    Terradue.Security.Certification.CertificateUser certUser = Terradue.Security.Certification.CertificateUser.FromId(context,Id);
                    return certUser.CertificateSubject;
                }
                catch ( EntityNotFoundException e ){
                    return null;
                }
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.TepQW.Controller.UserTep"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public UserT2(IfyContext context) : base(context) {
            oneClient = new OneClient(context.GetConfigValue("One-xmlrpc-url"),context.GetConfigValue("One-admin-usr"),context.GetConfigValue("One-admin-pwd"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.TepQW.Controller.UserTep"/> class.
        /// </summary>
        /// <param name="user">User.</param>
        public UserT2(IfyContext context, User user) : base(context) {
            this.Id = user.Id;
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
        public static new UserT2 FromId(IfyContext context, int id){
            UserT2 user = new UserT2(context);
            user.Id = id;
            user.Load();

            string sql = String.Format("SELECT username FROM usr_github WHERE id_usr={0};",id);
            user.GithubName = context.GetQueryStringValue(sql);
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

