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
        /// Indexs the exists.
        /// </summary>
        /// <returns><c>true</c>, if exists was indexed, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        public bool IndexExists(string index){
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Host + "/" + index);
            request.Method = "HEAD";

            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)request.GetResponse();
            } catch (WebException e) {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Found) {
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
        public void CreateIndex(string index){
            if(!IndexExists(index)){
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Host + "/" + index);
                request.Method = "PUT";
                //TODO: Add Credentials

//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            }
        }
    }
}

