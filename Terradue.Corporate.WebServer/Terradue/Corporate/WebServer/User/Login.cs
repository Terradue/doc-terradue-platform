using System;
using System.Data;
using System.ComponentModel;
using System.Collections.Generic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using Terradue.Portal;
using System.Web.SessionState;
using System.Diagnostics;
using ServiceStack;

namespace Terradue.Corporate.WebServer
{
    [Route("/auth", "POST", Summary = "login", Notes = "Login to the platform with username/password")]
    public class Login : IReturn<Terradue.WebService.Model.WebUser>
	{
		[ApiMember(Name="username", Description = "username", ParameterType = "path", DataType = "String", IsRequired = true)]
		public String username { get; set; }

		[ApiMember(Name="password", Description = "password", ParameterType = "path", DataType = "String", IsRequired = true)]
		public String password { get; set; }

	}

    [Route("/auth/openId", "GET", Summary = "GET a login", Notes = "Login to the platform with openId")]
    public class OpenIdLogin : IReturn<Terradue.WebService.Model.WebUser>
    {
        [ApiMember(Name="provider", Description = "provider", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String provider { get; set; }

        [ApiMember(Name="url", Description = "return to URL", ParameterType = "path", DataType = "String", IsRequired = true)]
        public String url { get; set; }
    }

    [Route("/auth", "DELETE", Summary = "logout", Notes = "Logout from the platform")]
	public class Logout : IReturn<String>
	{
	}

    [Route("/auth", "GET")]
    public class Auth {}

    [Route("/cb", "GET")]
    public class CallBack {
        [ApiMember(Name = "code", Description = "oauth code", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string Code { get; set; }

        [ApiMember(Name = "state", Description = "oauth state", ParameterType = "query", DataType = "string", IsRequired = true)]
        public string State { get; set; }

    }

}
