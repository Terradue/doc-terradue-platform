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
    [Api("Tep-QuickWin Terradue webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure   | EndpointAttributes.External | EndpointAttributes.Json)]
    public class SearchService : ServiceStack.ServiceInterface.Service
    {       
//        public object Get(GetAllSearch request){
//            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
//            IEnumerable<System.ServiceModel.Syndication.SyndicationFeed> result = new IEnumerable<System.ServiceModel.Syndication.SyndicationFeed>();
//            try{
//                context.Open();
//
//                // Load the complete request
//                HttpRequest httpRequest = HttpContext.Current.Request;
//                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;
//
//                string format;
//                if ( Request.QueryString["format"] == null ) format = "atom";
//                else format = Request.QueryString["format"];
//
//                Type responseType = MasterCatalogue.GetTypeFromFormat (format);
//                IOpenSearchResult osr;
//
//                List<Terradue.OpenSearch.IOpenSearchable> entities;
//
//                //services
//                EntityList<Terradue.Portal.WpsProcessOffering> services = new EntityList<Terradue.Portal.WpsProcessOffering>(context);
//                services.Load();
//                osr = ose.Query(services, httpRequest.QueryString, responseType);
//                result.
//                result.AddRange(osr.Result);
//
//                //series
//                MasterCatalogue cat = new MasterCatalogue(context);
//                osr = MasterCatalogue.OpenSearchEngine.Query(cat, httpRequest.QueryString, responseType);
//                result.AddRange(osr.Result);
//
//
//
//                context.Close ();
//            }catch(Exception e) {
//                context.Close ();
//                throw e;
//            }
//
//            return result;
//        }
    }
        [Route("/search", "GET", Summary = "GET the result of a search", Notes = "")]
        public class GetAllSearch
        {
            [ApiMember(Name="query", Description = "query", ParameterType = "query", DataType = "string", IsRequired = true)]
            public string query { get; set; }
        }
}

