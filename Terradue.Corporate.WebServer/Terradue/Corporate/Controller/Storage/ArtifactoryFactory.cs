using System;
using Terradue.Portal;
using System.Collections.Generic;
using System.Net;
using ServiceStack.Text;
using System.IO;
using Terradue.JFrog.Artifactory;

namespace Terradue.Corporate.Controller {
    public class ArtifactoryFactory {
        
        public IfyContext Context { get; set; }
        public JFrogArtifactoryClient JFrogClient { get; set; }

        public ArtifactoryFactory(IfyContext context) {
            this.Context = context;
            JFrogClient = new JFrogArtifactoryClient(context.GetConfigValue("artifactory-APIurl"), context.GetConfigValue("artifactory-APIkey"));
        }

        /***************************************************************************************************************************************/

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

        #endregion

        /***************************************************************************************************************************************/

        #region Group

        /// <summary>
        /// Creates the group.
        /// </summary>
        /// <param name="group">Group.</param>
        public void CreateGroup(string group){
            ArtifactoryGroup config = new ArtifactoryGroup();
            config.name = group;
            config.realm = "ARTIFACTORY";

            JFrogClient.CreateGroup(config);
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
                groups = new List<Dictionary<string, List<string>>>{ dico }
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
            JFrogClient = new JFrogArtifactoryClient(Context.GetConfigValue("artifactory-APIurl"), username, password);
            var apikey = JFrogClient.GetApiKey();

            //put admin config back
            JFrogClient = new JFrogArtifactoryClient(Context.GetConfigValue("artifactory-APIurl"), Context.GetConfigValue("artifactory-APIkey"));

            return apikey.apiKey;
        }

        /// <summary>
        /// Adds the API key.
        /// </summary>
        /// <param name="apikey">Apikey.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void AddApiKey(string apikey, string username, string password){
            //log in as the user
            JFrogClient = new JFrogArtifactoryClient(Context.GetConfigValue("artifactory-APIurl"), username, password);
            JFrogClient.CreateApiKey(apikey);

            //put admin config back
            JFrogClient = new JFrogArtifactoryClient(Context.GetConfigValue("artifactory-APIurl"), Context.GetConfigValue("artifactory-APIkey"));
        }

        /// <summary>
        /// Revokes the API key.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public void RevokeApiKey(string username, string password){
            //log in as the user
            JFrogClient = new JFrogArtifactoryClient(Context.GetConfigValue("artifactory-APIurl"), username, password);
            JFrogClient.RevokeApiKey();

            //put admin config back
            JFrogClient = new JFrogArtifactoryClient(Context.GetConfigValue("artifactory-APIurl"), Context.GetConfigValue("artifactory-APIkey"));
        }

        #endregion

        /***************************************************************************************************************************************/
    }
}

