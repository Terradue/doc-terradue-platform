using System;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Funq;
using ServiceStack;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;

namespace Terradue.Corporate.WebServer {
	
	//-------------------------------------------------------------------------------------------------------------------------
	//-------------------------------------------------------------------------------------------------------------------------
	//-------------------------------------------------------------------------------------------------------------------------
	/// <summary>Global class</summary>
	public class Global : HttpApplication {
		public AppHost appHost;

		/// <summary>Application initialisation</summary>
		protected void Application_Start(object sender, EventArgs e) {
			appHost = new AppHost();
			appHost.Init();
		}

		protected void Application_Error(object sender, EventArgs e) {
			Context.IsErrorResponse();
		}

		protected void Application_BeginRequest(object sender, EventArgs e) {
			string urlPath = Request.Path.ToLower();

			if (urlPath.StartsWith("/portal/") || urlPath.Equals("/portal"))
				HttpContext.Current.RewritePath("/index.html");
			else if (urlPath.Equals("/signin"))
				HttpContext.Current.RewritePath("/index.html");
		}
	}
}
