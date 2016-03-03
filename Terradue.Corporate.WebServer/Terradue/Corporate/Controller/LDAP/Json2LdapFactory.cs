using System;
using System.Net;
using Terradue.Portal;
using System.IO;
using ServiceStack.Text;
using Terradue.Ldap;

namespace Terradue.Corporate.Controller {
    public class Json2LdapFactory {

        public IfyContext Context { get; set; }
        public Json2LdapClient Json2Ldap { get; set; }

        public Json2LdapFactory(IfyContext context) {
            this.Context = context;
            this.Json2Ldap = new Json2LdapClient(context.GetConfigValue("ldap-baseurl"));
        }

        /// <summary>
        /// Creates the LDAP D.
        /// </summary>
        /// <returns>The LDAP D.</returns>
        /// <param name="uid">Uid.</param>
        public string CreateLdapDN(string uid) {
            string dn = string.Format("uid={0}, ou=people, dc=terradue, dc=com", uid);
            string dnn = Json2Ldap.NormalizeDN(dn);
            if (!Json2Ldap.IsValidDN(dnn))
                throw new Exception(string.Format("Unvalid DN: {0}", dnn));
            return dnn;
        }

        /// <summary>
        /// Determines whether this instance is unix username is free on ldap
        /// </summary>
        /// <returns><c>true</c> if this unix username is free; otherwise, <c>false</c>.</returns>
        /// <param name="username">Username.</param>
        public bool IsUsernameFree(string username) {
            bool result = false;

            //open the connection
            Json2Ldap.Connect();
            try {
                var dn = CreateLdapDN(username);

                var response = Json2Ldap.GetEntry(dn);
                if (response == null)
                    result = true;
            } catch (Exception e) {
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
            return result;
        }

        /// <summary>
        /// Loads the user from email.
        /// </summary>
        /// <returns>The user from email.</returns>
        /// <param name="email">Email.</param>
        public LdapUser GetUserFromEmail(string email){
            return GetUserFromFilter("email", email);
        }

        /// <summary>
        /// Loads the user from EOSSO.
        /// </summary>
        /// <returns>The user from EOSSO.</returns>
        /// <param name="eosso">Eosso.</param>
        public LdapUser GetUserFromEOSSO(string eosso){
            return GetUserFromFilter("eosso", eosso);
        }

        /// <summary>
        /// Loads the user from filter.
        /// </summary>
        /// <returns>The user from filter.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public LdapUser GetUserFromFilter(string key, string value){

            LdapUser usr = null;

            //open the connection
            Json2Ldap.Connect();
            try{
                string basedn = "ou=people, dc=terradue, dc=com";
                string filter = string.Format("({0}={1})",key, value);

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(Context.GetConfigValue("ldap-admin-dn"), Context.GetConfigValue("ldap-admin-pwd"));

                var response = Json2Ldap.SearchEntries(basedn, Json2LdapSearchScopes.SUB, filter);

                if (response.matches == null || response.matches.Count == 0) 
                    usr=null;
                else 
                    usr = new LdapUser(response.matches[0]);
            }catch(Exception e){
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
            return usr;
        }

    }
}

