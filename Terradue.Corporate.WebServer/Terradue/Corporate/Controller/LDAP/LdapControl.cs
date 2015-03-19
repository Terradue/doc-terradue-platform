using System;
using System.Net;
using Terradue.Portal;
using System.IO;
using ServiceStack.Text;

namespace Terradue.Corporate.WebServer {
    public class LdapControl {

        public IfyContext Context { get; set; }
        public string CID { get; set; }


        private string LdapHost { get; set; }


        public LdapControl(IfyContext context) {
            this.Context = context;
            this.LdapHost = context.GetConfigValue("ldap-baseurl");
        }

        public void Connect(){

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(LdapHost));
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            string json = "{" +
                "\"method\":\"ldap.connect\"," +
                "\"params\":{\"host\":\"" + this.LdapHost + "\",\"port\":389}," +
                "\"id\":1," +
                "\"jsonrpc\":\"2.0\"" +
                "}";    

            using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                    string result = streamReader.ReadToEnd();
                    try{
                        ConnectResponse response = JsonSerializer.DeserializeFromString<ConnectResponse>(result);
                        if(response.error != null) throw new Exception(string.Format("{0} - {1}", response.error.message, response.error.code));
                        else{
                            this.CID = response.result.CID;
                        }
                    }catch(Exception e){
                                throw e;
                    }
                }
            }
        }

    }
}

