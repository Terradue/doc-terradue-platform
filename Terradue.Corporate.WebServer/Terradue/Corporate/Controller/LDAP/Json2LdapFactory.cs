using System;
using System.Net;
using Terradue.Portal;
using System.IO;
using ServiceStack.Text;
using Terradue.Ldap;
using System.Collections.Generic;

namespace Terradue.Corporate.Controller {
    public class Json2LdapFactory {

        public IfyContext Context { get; set; }
        public Json2LdapClient Json2Ldap { get; set; }

        public Json2LdapFactory(IfyContext context) {
            this.Context = context;
            this.Json2Ldap = new Json2LdapClient(context.GetConfigValue("ldap-baseurl"));
        }

        /// <summary>
        /// Creates the LDAP DN.
        /// </summary>
        /// <returns>The LDAP DN.</returns>
        /// <param name="uid">Uid.</param>
        public string CreateLdapPeopleDN(string uid) {
            string dn = string.Format("uid={0}, ou={1}, dc={2}, dc={3}", uid, "people", "terradue", "com");
            return NormalizeLdapDN(dn);
        }

        public string CreateLdapT2DN(string uid, string ou) {
            string dn = string.Format("uid={0}, ou={1}, dc={2}, dc={3}", uid, ou, "terradue", "com");
            return NormalizeLdapDN(dn);
        }

        public string CreateLdapT2DN(string uid, List<string> ou) {
            var ous = "";
            foreach (var entry in ou)
                ous += ", ou=" + entry;
            string dn = string.Format("uid={0}{1}, dc={2}, dc={3}", uid, ous, "terradue", "com");
            return NormalizeLdapDN(dn);
        }

        public string NormalizeLdapDN(string dn) {
            string dnn = Json2Ldap.NormalizeDN(dn);
            if (!Json2Ldap.IsValidDN(dnn))
                throw new Exception(string.Format("Unvalid DN: {0}", dnn));
            return dnn;
        }

        /// <summary>
        /// Test if Users exists.
        /// </summary>
        /// <returns><c>true</c>, if exists was usered, <c>false</c> otherwise.</returns>
        /// <param name="username">Username.</param>
        public bool UserExists(string username) {
            bool result = false;

            //open the connection
            Json2Ldap.Connect();
            try {
                var dn = CreateLdapPeopleDN(username);

                var response = Json2Ldap.GetEntry(dn);
                if (response != null) result = true;
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
            return GetUserFromFilter("mail", email);
        }

        /// <summary>
        /// Loads the user from EOSSO.
        /// </summary>
        /// <returns>The user from EOSSO.</returns>
        /// <param name="eosso">Eosso.</param>
        public LdapUser GetUserFromEOSSO(string eosso){
            return GetUserFromFilter("eossoUserid", eosso);
        }

        /// <summary>
        /// Gets the user from uid.
        /// </summary>
        /// <returns>The user from uid.</returns>
        /// <param name="uid">Uid.</param>
        public LdapUser GetUserFromUid(string uid){
            return GetUserFromFilter("uid", uid);
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

        public List<LdapUser> GetUsersFromLdap(){
            List<LdapUser> usrs = new List<LdapUser>();
            //open the connection
            Json2Ldap.Connect();
            try{
                string basedn = "ou=people, dc=terradue, dc=com";

                //login as ldap admin to have creation rights
                Json2Ldap.SimpleBind(Context.GetConfigValue("ldap-admin-dn"), Context.GetConfigValue("ldap-admin-pwd"));

                var response = Json2Ldap.SearchEntries(basedn, Json2LdapSearchScopes.SUB, "(objectClass=*)");

                if (response.matches == null || response.matches.Count == 0) 
                    usrs=null;
                else {
                    foreach(var entry in response.matches)
                        if(entry.uid != null) usrs.Add(new LdapUser(entry));
                }
            }catch(Exception e){
                Json2Ldap.Close();
                throw e;
            }
            Json2Ldap.Close();
            return usrs;
        }

    }
}

