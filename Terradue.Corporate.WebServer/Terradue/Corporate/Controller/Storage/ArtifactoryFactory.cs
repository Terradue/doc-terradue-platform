using System;
using Terradue.Portal;

namespace Terradue.Corporate.Controller {
    public class ArtifactoryFactory {
        public IfyContext Context { get; set; }

        public ArtifactoryFactory(IfyContext context) {
            this.Context = context;
        }

        public bool RepositoryExists(string repo){
            return false;    
        }

        public void RepositoryCreate(string repo){
            if(!RepositoryExists(repo)){
                //create repo on Artifactory
                //import group from LDAP
                //Create permission for group on new repo
                //Insert Api Key ds artifactory
            }
        }

        public void AddApiKey(string apikey){
            
        }
    }
}

