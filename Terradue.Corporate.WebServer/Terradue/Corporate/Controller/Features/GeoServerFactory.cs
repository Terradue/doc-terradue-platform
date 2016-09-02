using System;
using System.IO;
using Terradue.Portal;
using System.Net;
using System.Runtime.Serialization;
using ServiceStack.Text;

namespace Terradue.Corporate.Controller {
    public class GeoServerFactory {

        public IfyContext Context { get; set; }
        public string Host { get; set; }
        private NetworkCredential Credentials { get; set; }

        public GeoServerFactory(IfyContext context) {
            this.Context = context;
            Host = context.GetConfigValue("geoserver-BaseUrl");
            Credentials = new NetworkCredential {
                UserName = context.GetConfigValue("geoserver-admin-usr"),
                Password = context.GetConfigValue("geoserver-admin-pwd")
            };
        }

        /// <summary>
        /// Check if the Workspace exists.
        /// </summary>
        /// <returns><c>true</c>, if the worspace exists, <c>false</c> otherwise.</returns>
        /// <param name="workspace">Workspace.</param>
        public bool WorkspaceExists(string workspace){
            var request = (HttpWebRequest)WebRequest.Create(this.Host + "/workspaces/" + workspace);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Credentials = Credentials;

            try{
//                var httpResponse = (HttpWebResponse)request.GetResponse();
//                if(httpResponse.StatusCode == HttpStatusCode.NotFound) return false;
                return true;
            }catch(Exception){
                return false;
            }
        }

        /// <summary>
        /// Creates the workspace.
        /// </summary>
        /// <param name="name">Name.</param>
        public void CreateWorkspace(string name){
            var request = (HttpWebRequest)WebRequest.Create(this.Host + "/workspaces");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Credentials = Credentials;

            GeoServerWorkspace workspace = new GeoServerWorkspace();
            workspace.workspace = new GeoServerWorkspaceConfif();
            workspace.workspace.name = name;

            string json = JsonSerializer.SerializeToString<GeoServerWorkspace>(workspace);

            using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                try{
//                    var httpResponse = (HttpWebResponse)request.GetResponse();
                }catch(Exception e){
                    throw e;
                }
            }
        }
    }

    [DataContract]
    public class GeoServerWorkspace {

        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        /// <value>The workspace.</value>
        [DataMember]
        public GeoServerWorkspaceConfif workspace { get; set; }
    }

    [DataContract]
    public class GeoServerWorkspaceConfif {

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DataMember]
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the data stores.
        /// </summary>
        /// <value>The data stores.</value>
        [DataMember]
        public GeoServerStore dataStores { get; set; }

        /// <summary>
        /// Gets or sets the coverage stores.
        /// </summary>
        /// <value>The coverage stores.</value>
        [DataMember]
        public GeoServerStore coverageStores { get; set; }

        /// <summary>
        /// Gets or sets the wms stores.
        /// </summary>
        /// <value>The wms stores.</value>
        [DataMember]
        public GeoServerStore wmsStores { get; set; }
    }

    [DataContract]
    public class GeoServerStore {

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        /// <value>The link.</value>
        [DataMember]
        public string link { get; set; }
    }
}

