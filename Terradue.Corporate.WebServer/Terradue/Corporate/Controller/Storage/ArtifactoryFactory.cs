using System;
using Terradue.Portal;
using System.Collections.Generic;
using System.Net;
using ServiceStack.Text;
using System.IO;
using Terradue.JFrog.Artifactory;

namespace Terradue.Corporate.Controller {
    public class ArtifactoryFactory {

        private string AdminApiKey { get; set; }

        public IfyContext Context { get; set; }
        public JFrogArtifactoryClient JFrogClient { get; set; }

        public ArtifactoryFactory(IfyContext context) {
            this.Context = context;
            this.AdminApiKey = this.Context.GetConfigValue("artifactory-APIkey");
            JFrogClient = new JFrogArtifactoryClient(context.GetConfigValue("artifactory-APIurl"), AdminApiKey);
            JFrogClient.SyncUrl = context.GetConfigValue("artifactory-SyncUrl");;
        }

        /***************************************************************************************************************************************/

        #region User

        /// <summary>
        /// Sync the specified username and password.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void Sync(string username, string password){
            //log in as the user
            JFrogClient.SetUserAuthentication(username, password);
            JFrogClient.Sync();

            //put admin config back
            JFrogClient.SetApiKeyAuthentication();
        }

        /// <summary>
        /// Gets the user info.
        /// </summary>
        /// <returns>The user info.</returns>
        /// <param name="username">Username.</param>
        public ArtifactoryUser GetUserInfo(string username){
            return JFrogClient.GetUser(username);
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="username">Username.</param>
        public void DeleteUser (string username) {
            ArtifactoryUser user = new ArtifactoryUser { 
                name = username
            };
            JFrogClient.DeleteUser (user);
        }

        #endregion

        #region Repository

        /// <summary>
        /// Creates the local repository.
        /// </summary>
        /// <param name="repo">Repo.</param>
        /// <param name="removeOld">If set to <c>true</c> remove old.</param>
        public void CreateLocalRepository(string repo, bool removeOld = false){
            if (removeOld || !RepositoryExists(repo)) {
                ArtifactoryRepositoryLocalConfiguration config = new ArtifactoryRepositoryLocalConfiguration();
                config.rclass = JFrogArtifactoryClient.REPOSITORY_LOCAL;
                config.key = repo;
                config.packageType = "generic";
                config.handleReleases = true;
                config.handleSnapshots = true;
                config.snapshotVersionBehavior = "non-unique";
                config.dockerApiVersion = "V2";

                JFrogClient.CreateRepository<ArtifactoryRepositoryLocalConfiguration>(config, repo);
            }
        }

        /// <summary>
        /// Repositories the exists.
        /// </summary>
        /// <returns><c>true</c>, if exists was repositoryed, <c>false</c> otherwise.</returns>
        /// <param name="repo">Repo.</param>
        public bool RepositoryExists(string repo){
            try{
                JFrogClient.GetRepository(repo);
            }catch(Exception){
                return false;   
            }
            return true;
        }

        /// <summary>
        /// Gets the storage info.
        /// </summary>
        /// <param name="repo">Repo.</param>
        public RepositoriesSummary GetStorageInfo (string repo) {
            if (string.IsNullOrEmpty (repo)) throw new Exception ("Invalid storage name : " + repo);
            ArtifactoryStorageInfo info = JFrogClient.StorageInfo ();
            foreach (var storage in info.repositoriesSummaryList) {
                if (repo.Equals(storage.repoKey)) {
                    return storage;
                }
            }
            return null;
        }

        #endregion

        /***************************************************************************************************************************************/

        #region Group

        /// <summary>
        /// Creates the group.
        /// </summary>
        /// <param name="group">Group.</param>
        public void CreateGroup(string group, string dn){
            ArtifactoryGroup config = new ArtifactoryGroup();
            config.name = group;
            config.description = "Group synchronized from LDAP";
            config.realm = "ldap";
            config.autojoin = false;
            config.realmAttributes = string.Format("ldapGroupName={0};groupsStrategy=STATIC;groupDn={1}", group, dn);

            JFrogClient.CreateGroup(config);
        }

        /// <summary>
        /// Gets the groups for user.
        /// </summary>
        /// <returns>The groups for user.</returns>
        /// <param name="username">Username.</param>
        public List<string> GetGroupsForUser(string username){
            ArtifactoryUser userinfo = GetUserInfo(username);
            return userinfo.groups;
        }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <returns>The groups.</returns>
        public List<string> GetGroups(){
            List<string> groups = new List<string>();
            foreach (var g in JFrogClient.ListGroups())
                groups.Add(g.name);
            return groups;
        }

        #endregion

        /***************************************************************************************************************************************/

        #region Permissions

        /// <summary>
        /// Creates the permission for group on repo.
        /// </summary>
        /// <param name="group">Group.</param>
        /// <param name="repo">Repo.</param>
        public void CreatePermissionForGroupOnRepo(string group, string repo){

            Dictionary<string, List<string>> dico = new Dictionary<string, List<string>>();
            dico.Add(group, new List<string>{ JFrogArtifactoryClient.PERMISSION_ADMIN, JFrogArtifactoryClient.PERMISSION_DELETE, JFrogArtifactoryClient.PERMISSION_READ });

            ArtifactoryPermission config = new ArtifactoryPermission();
            config.name = group;
            config.repositories = new List<string>{ repo };
            config.principals = new ArtifactoryPermissionPrincipal { 
                groups = dico
            };

            JFrogClient.CreatePermission(config);
        }

        #endregion

        /***************************************************************************************************************************************/

        #region APIKEY

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <returns>The API key.</returns>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public string GetApiKey(string username, string password){
            JFrogClient.SetUserAuthentication(username, password);
            var apikey = JFrogClient.GetApiKey();

            //put admin config back
            JFrogClient.SetApiKeyAuthentication();

            return apikey.apiKey;
        }

        /// <summary>
        /// Creates the API key.
        /// </summary>
        /// <returns>The API key.</returns>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public string CreateApiKey(string username, string password){
            //log in as the user
            JFrogClient.SetUserAuthentication(username, password);

            //test if key already exists
            ArtifactoryApiKey key = null;
            try {
                key = JFrogClient.GetApiKey ();
                JFrogClient.RevokeApiKey ();
            } catch (Exception){}

            try {
                key = JFrogClient.CreateApiKey ();
            } catch (Exception e){
                Context.LogError (this, e.Message + "-" + e.StackTrace);
            }

            //put admin config back
            JFrogClient.SetApiKeyAuthentication();

            if (key != null)
                return key.apiKey;
            else return null;
        }

        /// <summary>
        /// Revokes the API key.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void RevokeApiKey(string username, string password){
            //log in as the user
            JFrogClient.SetUserAuthentication(username, password);
            JFrogClient.RevokeApiKey();

            //put admin config back
            JFrogClient.SetApiKeyAuthentication();
        }

        #endregion

        /***************************************************************************************************************************************/
    }
}

