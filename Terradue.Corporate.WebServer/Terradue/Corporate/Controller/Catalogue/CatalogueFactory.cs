using System;
using Terradue.Portal;
using System.Net;

namespace Terradue.Corporate.Controller {
    public class CatalogueFactory {

        public IfyContext Context { get; set; }
        public string Host { get; set; }

        public CatalogueFactory(IfyContext context) {
            this.Context = context;
            Host = context.GetConfigValue("catalogue-host");
        }

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

        public void IndexCreate(string index){
            if(!IndexExists(index)){
                
            }
        }
    }
}

