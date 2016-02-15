using System;
using ServiceStack.ServiceHost;
using Terradue.TepQW.WebServer.Common;
using Terradue.Portal;
using System.Web;
using Terradue.OpenSearch.Engine;
using Terradue.TepQW.Controller;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;

namespace Terradue.TepQW.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure   | EndpointAttributes.External | EndpointAttributes.Json)]
    public class SearchService : ServiceStack.ServiceInterface.Service
    {       
    }
        [Route("/search", "GET", Summary = "GET the result of a search", Notes = "")]
        public class GetAllSearch
        {
            [ApiMember(Name="query", Description = "query", ParameterType = "query", DataType = "string", IsRequired = true)]
            public string query { get; set; }
        }
}

