using System;
using System.Collections.Generic;
using System.Web;
using ServiceStack.Common.Web;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.Portal;
using Terradue.Corporate.Controller;
using Terradue.WebService.Model;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.Corporate.WebServer {

    public class ServiceService : ServiceStack.ServiceInterface.Service {
        
        public object Get(SearchServices request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            object result;
            try{
                context.Open();
                EntityList<Terradue.Portal.Service> services = new EntityList<Terradue.Portal.Service>(context);
                services.Load();

                // Load the complete request
                HttpRequest httpRequest = HttpContext.Current.Request;

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;

                string format;
                if ( Request.QueryString["format"] == null ) format = "atom";
                else format = Request.QueryString["format"];

                Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);

                IOpenSearchResult osr = ose.Query(services, httpRequest.QueryString, type);

                result = osr.Result;

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

        public object Get(SearchWPSServices request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            context.RestrictedMode = false;
            object result;
            try{
                context.Open();
                EntityList<Terradue.Portal.WpsProcessOffering> services = new EntityList<Terradue.Portal.WpsProcessOffering>(context);
                services.Load();

                // Load the complete request
                HttpRequest httpRequest = HttpContext.Current.Request;

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;

                string format;
                if ( Request.QueryString["format"] == null ) format = "atom";
                else format = Request.QueryString["format"];

                Type responseType = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);
                IOpenSearchResult osr = ose.Query(services, httpRequest.QueryString, responseType);

                result = osr.Result;

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

    }
}

