using System;
using System.Collections;
using System.Collections.Specialized;
using System.ServiceModel.Syndication;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using Terradue.Portal;
using Terradue.Corporate.Controller;
using Terradue.WebService.Model;
using Terradue.Corporate.WebServer.Common;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;
using ServiceStack.Text;
using Terradue.OpenSearch;

namespace Terradue.Corporate.WebServer
{
    [Api("Terradue Corporate webserver")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
		EndpointAttributes.Secure   | EndpointAttributes.External | EndpointAttributes.Json)]
	public class CatalogueService : ServiceStack.ServiceInterface.Service
	{		
		/*				! 
		 * \fn Get(Getseries request)
		 * \brief Response to the Get request with a Getseries object (get the complete list of series)
		 * \param request request content
		 * \return the series list
		 */
		public object Get(GetOpensearchDescription request)
		{
			OpenSearchDescription OSDD;
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
			try{
				context.Open();
				UriBuilder baseUrl = new UriBuilder ( context.BaseUrl );

				if ( request.serieId == null )
                    throw new ArgumentNullException(Terradue.Corporate.WebServer.CustomErrorMessages.WRONG_IDENTIFIER);
					
                Terradue.Corporate.Controller.DataSeries serie = Terradue.Corporate.Controller.DataSeries.FromIdentifier(context,request.serieId);

				// The new URL template list 
				Hashtable newUrls = new Hashtable();
				UriBuilder urib;
				NameValueCollection query = new NameValueCollection();
				string[] queryString;

				urib = new UriBuilder( baseUrl.ToString() );

                OSDD = serie.GetOpenSearchDescription();
				urib.Path = baseUrl.Path + "/catalogue/" + serie.Identifier + "/search";
				query.Set("format","atom");
                query.Add(serie.GetOpenSearchParameters("application/atom+xml"));

				queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
				urib.Query = string.Join("&", queryString);
				newUrls.Add("application/atom+xml",new OpenSearchDescriptionUrl("application/atom+xml", urib.ToString(), "search"));

				query.Set("format","json");
				queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
				urib.Query = string.Join("&", queryString);
				newUrls.Add("application/json",new OpenSearchDescriptionUrl("application/json", urib.ToString(), "search"));

				query.Set("format","html");
				queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
				urib.Query = string.Join("&", queryString);
				newUrls.Add("text/html",new OpenSearchDescriptionUrl("application/html", urib.ToString(), "search"));
				OSDD.Url = new OpenSearchDescriptionUrl[newUrls.Count];

				newUrls.Values.CopyTo(OSDD.Url,0);
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}
			HttpResult hr = new HttpResult ( OSDD, "application/opensearchdescription+xml" );
			return hr;
		}

		/// <summary>
		/// Get the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Get(GetOpensearchDescriptions request){
			OpenSearchDescription OSDD;
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
			try{
				context.Open();

                MasterCatalogue cat = new MasterCatalogue(context);
                OSDD = cat.GetOpenSearchDescription();
				
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}
			HttpResult hr = new HttpResult ( OSDD, "application/opensearchdescription+xml" );
			return hr;
		}

		/// <summary>
		/// Get the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Get(GetOpensearchSearch request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            IOpenSearchResult result = null;
			try{
				context.Open();
				// Load the complete request
                HttpRequest httpRequest = HttpContext.Current.Request;

              	if ( request.serieId == null )
                    throw new ArgumentNullException(Terradue.Corporate.WebServer.CustomErrorMessages.WRONG_IDENTIFIER);

                Terradue.Corporate.Controller.DataSeries serie = Terradue.Corporate.Controller.DataSeries.FromIdentifier(context,request.serieId);

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;
                ose.DefaultTimeOut = 60000;

                Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);

                result = ose.Query(serie, httpRequest.QueryString, type);

				context.Close ();

			}catch(Exception e) {
				context.Close ();
                throw e;
			}

            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
		}

		/// <summary>
		/// Get the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Get(GetOpensearchSearchs request){
			// This page is public

			// But its content will be adapted accrding to context (user id, ...)

			// Load the complete request
			HttpRequest httpRequest = HttpContext.Current.Request;
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            IOpenSearchResult result = null;
			try{
				context.Open();

                MasterCatalogue cat = new MasterCatalogue(context);
                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;
                ose.DefaultTimeOut = 60000;

                Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);
                result = ose.Query(cat, httpRequest.QueryString, type);		

				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}

            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
		}

        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(DataPackageSearchRequest request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            IOpenSearchResult result = null;
            try {
                context.Open();
                Terradue.Corporate.Controller.DataPackage datapackage;
                if(!context.IsUserAuthenticated && request.Key != null){
                    context.RestrictedMode = false;
                    datapackage = Terradue.Corporate.Controller.DataPackage.FromIdentifier(context, request.DataPackageId);
                    if(!request.Key.Equals(datapackage.AccessKey)) throw new UnauthorizedAccessException(CustomErrorMessages.WRONG_ACCESSKEY);
                } else {
                    datapackage = Terradue.Corporate.Controller.DataPackage.FromIdentifier(context, request.DataPackageId);
                }

                datapackage.SetOpenSearchEngine(MasterCatalogue.OpenSearchEngine);

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;

                Type responseType = OpenSearchFactory.ResolveTypeFromRequest(HttpContext.Current.Request,ose);
                result = ose.Query(datapackage, Request.QueryString, responseType);

                context.Close();

            } catch (Exception e) {
                context.Close();
                throw e;
            }

            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
        }


        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(DataPackageDescriptionRequest request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();
                Terradue.Corporate.Controller.DataPackage datapackage;
                if(!context.IsUserAuthenticated && request.Key != null){
                    context.RestrictedMode = false;
                    datapackage = Terradue.Corporate.Controller.DataPackage.FromIdentifier(context, request.DataPackageId);
                    if(!request.Key.Equals(datapackage.AccessKey)) throw new UnauthorizedAccessException(CustomErrorMessages.WRONG_ACCESSKEY);
                } else
                    datapackage = Terradue.Corporate.Controller.DataPackage.FromIdentifier(context, request.DataPackageId);

                OpenSearchDescription osd = datapackage.GetLocalOpenSearchDescription();

                context.Close();

                return new HttpResult(osd,"application/opensearchdescription+xml");
            } catch (Exception e) {
                context.Close();
                throw e;
            }
        }

	}

}