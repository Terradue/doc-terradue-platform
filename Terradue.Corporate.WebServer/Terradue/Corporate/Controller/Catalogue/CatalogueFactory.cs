using System;
using Terradue.Portal;
using System.Net;

namespace Terradue.Corporate.Controller {
    public class CatalogueFactory {

        public IfyContext Context { get; set; }
        public string Host { get; set; }

        public CatalogueFactory(IfyContext context) {
            this.Context = context;
            Host = context.GetConfigValue("catalogue-BaseUrl");
        }

        /// <summary>
        /// Gets the user index URL.
        /// </summary>
        /// <returns>The user index URL.</returns>
        /// <param name="index">Index.</param>
        public string GetUserIndexUrl (string index) {
            return this.Host + "/" + index;
        }

        /// <summary>
        /// Tests if the index exists
        /// </summary>
        /// <returns><c>true</c>, if exists was indexed, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        /// <param name="username">Username.</param>
        /// <param name="apikey">Apikey.</param>
        public bool IndexExists(string index, string username, string apikey){
            if (string.IsNullOrEmpty (index)) throw new Exception ("Index not created, invalid index");
            if (string.IsNullOrEmpty (username)) throw new Exception ("Index not created, invalid username");
            if (string.IsNullOrEmpty (apikey)) throw new Exception ("Index not created, invalid API KEY");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Host + "/" + index + "/description");
            request.Method = "GET";
            request.Credentials = new NetworkCredential (username, apikey);
            request.PreAuthenticate = true;

            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException e) {
                if ((e.Response != null && ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Found)) {
                    return true;
                }
                return false;
            } catch (Exception){
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates the index.
        /// </summary>
        /// <param name="index">Index.</param>
        public void CreateIndex(string index, string username, string apikey){
            if (string.IsNullOrEmpty (index)) throw new Exception ("Index not created, invalid index");
            if (string.IsNullOrEmpty (username)) throw new Exception ("Index not created, invalid username");
            if (string.IsNullOrEmpty (apikey)) throw new Exception ("Index not created, invalid API KEY");

            if(!IndexExists(index, username, apikey)){
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Host + "/" + index);
                request.Method = "PUT";
                request.Credentials = new NetworkCredential (username, apikey);
                request.GetResponse();
            }
        }
    }
}

