using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ServiceStack.ServiceHost;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.Corporate.WebServer {

    [Route("/log", "GET", Summary = "GET report", Notes = "")]
    public class LogGetRequest : IReturn<string>{
        [ApiMember(Name = "filename", Description = "filename", ParameterType = "query", DataType = "string", IsRequired = false)]
        public string filename { get; set; }
    }

    [Route("/logs", "GET", Summary = "GET existing reports", Notes = "")]
    public class LogsGetRequest : IReturn<List<string>>{}

    /// <summary>
    /// Email confirmation service
    /// </summary>
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class LogService : ServiceStack.ServiceInterface.Service {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public object Get(LogsGetRequest request){
            var context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            List<string> result = new List<string>();
            try {
                context.Open();
                context.LogInfo(this,string.Format("/logs GET"));

                string path = AppDomain.CurrentDomain.BaseDirectory;
                if(!path.EndsWith("/")) path += "/";

                context.LogDebug(this, path);
                result = System.IO.Directory.GetFiles(path + "/logs","*.log*").ToList();
                result = result.ConvertAll(f => f.Substring(f.LastIndexOf("/") + 1));

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message);
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(LogGetRequest request){
            var context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            var text = new System.Text.StringBuilder();

            try {
                context.Open();
                context.LogInfo(this,string.Format("/log GET filename='{0}'", request.filename));

                string path = AppDomain.CurrentDomain.BaseDirectory;
                if(!path.EndsWith("/")) path += "/";
                var filepath = string.Format("{1}logs/{0}",request.filename,path);

                List<string> lines = new List<string>();
                using (var csv = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(csv)){
                    while (!sr.EndOfStream) {
                        var line = sr.ReadLine ();
                        lines.Add (line);
                    }
                }

                List<string> lines2 = new List<string> ();
                for (int i = 0; i < lines.Count; i++) {
                    if (lines [i].Contains ("ERROR")){
                        var afterline = lines [i + 1];
                        if (!afterline.Contains ("INFO") && !afterline.Contains ("DEBUG") && !afterline.Contains ("ERROR")){
                            lines2.Add (lines [i] + " - " + lines [++i]);
                            continue;
                        }
                    }
                    lines2.Add (lines [i]);
                }

                lines2.Reverse ();
                context.Close();
                return lines2.ToArray();
            } catch (Exception e) {
                context.LogError(this, e.Message);
                context.Close();
                throw e;
            }
        }

       }
}

