using System;
using System.Net;
using Terradue.Portal;
using System.IO;
using ServiceStack.Text;
using Terradue.Ldap;

namespace Terradue.Corporate.WebServer {
    public class Json2LdapFactory {

        public IfyContext Context { get; set; }
        public Json2LdapClient Json2Ldap { get; set; }


        public Json2LdapFactory(IfyContext context) {
            this.Context = context;
            this.Json2Ldap = new Json2LdapClient(context.GetConfigValue("ldap-baseurl"));
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

            //open the connection
            Json2Ldap.Connect();

            string basedn = "ou=people, dc=terradue, dc=com";
            string filter = string.Format("({0}={1})",key, value);

            //login as ldap admin to have creation rights
            Json2Ldap.SimpleBind(Context.GetConfigValue("ldap-admin-dn"), Context.GetConfigValue("ldap-admin-pwd"));

            var response = Json2Ldap.SearchEntries(basedn, Json2LdapSearchScopes.SUB, filter);

            if (response.matches == null || response.matches.Count == 0) return null;

            Json2Ldap.Close();
            return new LdapUser(response.matches[0]);
        }

    }
}

