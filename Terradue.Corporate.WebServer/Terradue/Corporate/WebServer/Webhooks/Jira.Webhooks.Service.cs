using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using Terradue.Corporate.Controller;
using Terradue.WebService.Model;
using System.Net;
using System.Web;
using System.IO;
using HtmlAgilityPack;
using System.Linq;

namespace Terradue.Corporate.WebServer {
	[Api("Terradue Corporate webserver")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
			  EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
	public class JiraWebhookService : ServiceStack.ServiceInterface.Service {

		public object Get(CreateDiscussPostFromJiraRelease request) {
			if (string.IsNullOrEmpty(request.Version)) throw new Exception("Unable to post new topic, version is null");
			if (string.IsNullOrEmpty(request.ProjectId)) throw new Exception("Unable to post new topic, project is null");

			IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
			try {
				context.Open();

				//Create JIRA release note
				string jiraUsername = context.GetConfigValue("jiraUsername");
                string jiraPassword = context.GetConfigValue("jiraPassword");
                string jiraReleaseUrl = string.Format("{0}?version={1}&styleName={2}&projectId={3}&Create=Create",
                                                      context.GetConfigValue("jiraReleaseBaseUrl"),
                                                      request.Version,
                                                     "Html",
                                                      request.ProjectId);
                
				HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(jiraReleaseUrl);
				webrequest.Method = "GET";
				webrequest.ContentType = "text/html";
				webrequest.Proxy = null;
                webrequest.Credentials = new NetworkCredential(jiraUsername, jiraPassword);
                webrequest.PreAuthenticate = true;

                string subject = "Ellip Release Notes - Version " + request.Version;
                string body = "";

				using (var httpResponse = (HttpWebResponse)webrequest.GetResponse()) {
					using (var stream = httpResponse.GetResponseStream()) {
                        //Extract infos from html
						HtmlDocument doc = new HtmlDocument();
						doc.Load(stream);
						
                        //get title
                        var itemList = doc.DocumentNode.SelectNodes("//title")//this xpath selects all textarea tag
										  .Select(p => p.InnerText)
										  .ToList();
						if (itemList.Count == 0) throw new Exception("No title found");
                        subject = itemList[itemList.Count - 1];
                        subject = subject.Replace(" - HTML format", "");

                        //get textarea
                        itemList = doc.DocumentNode.SelectNodes("//textarea")//this xpath selects all textarea tag
                                      .Select(p => p.InnerHtml)
										  .ToList();
                        if (itemList.Count == 0) throw new Exception("No textarea found");
                        body = itemList[0];
                        body = body.Replace(" <h1>", "<h1>");//Bug on discuss, do this to display correctly the h1
					}
				}

                //Create DISCUSS post
				string category = context.GetConfigValue("discussCategoryId-ellipRelease");
				string host = context.GetConfigValue("discussBaseUrl");
				string apiKey = context.GetConfigValue("discussApiKey");
				string apiUsername = context.GetConfigValue("discussApiUsername");

				webrequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}/posts", host));
				webrequest.Method = "POST";
				webrequest.ContentType = "application/x-www-form-urlencoded";
				webrequest.Proxy = null;

				var dataStr = string.Format("api_key={0}&api_username={1}&category={2}&title={3}&raw={4}", apiKey, apiUsername, category, HttpUtility.UrlEncode(subject), HttpUtility.UrlEncode(body));
				byte[] data = System.Text.Encoding.UTF8.GetBytes(dataStr);

                webrequest.ContentLength = data.Length;

				using (var requestStream = webrequest.GetRequestStream()) {
					requestStream.Write(data, 0, data.Length);
					requestStream.Close();
                    webrequest.GetResponse();
				}

				context.Close();
			} catch (Exception e) {
				context.Close();
				throw e;
			}
            return true;
		}

	}

	[Route("/webhooks/jira/discuss/release", "GET", Summary = "GET the current roles", Notes = "")]
    public class CreateDiscussPostFromJiraRelease : IReturn<bool> {
		[ApiMember(Name = "projectId", Description = "Jira project id", ParameterType = "query", DataType = "string", IsRequired = true)]
		public string ProjectId { get; set; }

		[ApiMember(Name = "version", Description = "Jira release version", ParameterType = "query", DataType = "string", IsRequired = true)]
		public string Version { get; set; }
    }
}

