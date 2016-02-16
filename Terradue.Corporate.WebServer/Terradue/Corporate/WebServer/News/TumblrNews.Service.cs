using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using Terradue.Corporate.Controller;
using Terradue.News;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Tumblr;
using Terradue.Portal;
using Terradue.WebService.Model;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class TumblrNewsService : ServiceStack.ServiceInterface.Service {

        [AddHeader(ContentType="application/atom+xml")]
        public object Get(SearchTumblrNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            IOpenSearchResultCollection result = null;
            try{
                context.Open();

                // Load the complete request
                HttpRequest httpRequest = HttpContext.Current.Request;

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;

                Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);

                List<TumblrFeed> tumblrs = TumblrNews.LoadTumblrFeeds(context);

                MultiGenericOpenSearchable multiOSE = new MultiGenericOpenSearchable(tumblrs.Cast<IOpenSearchable>().ToList(), ose);

                result = ose.Query(multiOSE, httpRequest.QueryString, type);

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return new HttpResult(result.SerializeToString(), result.ContentType);
        }

        public object Get(GetTumblrNewsFeeds request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            List<WebNews> result = new List<WebNews>();
            try{
                context.Open();

                List<TumblrFeed> tumblrs = TumblrNews.LoadTumblrFeeds(context);
                List<TumblrNews> tumblrfeeds = new List<TumblrNews>();
                foreach(TumblrFeed tumblr in tumblrs) tumblrfeeds.AddRange(TumblrNews.FromFeeds(context, tumblr.GetFeeds()));
                foreach(TumblrNews feed in tumblrfeeds) result.Add(new WebNews(feed));
                
                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

        public object Get(GetAllTumblrNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            List<WebNews> result = new List<WebNews>();
            try{
                context.Open();

                EntityList<TumblrNews> articles = new EntityList<TumblrNews>(context);
                articles.Load();
                foreach(TumblrNews article in articles) result.Add(new WebNews(article));

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

        public object Post(CreateTumblrNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            WebNews result = null;
            try{
                context.Open();

                TumblrNews article = new TumblrNews(context);
                article = (TumblrNews)request.ToEntity(context, article);
                article.Store();
                result = new WebNews(article);

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

    }

    [Route("/tumblr", "GET", Summary = "GET a list of tumblr news", Notes = "")]
    public class GetAllTumblrNews : IReturn<List<WebNews>>{}

    [Route("/tumblr/feeds", "GET", Summary = "GET a list of tumblr news feeds", Notes = "")]
    public class GetTumblrNewsFeeds : IReturn<List<WebNews>>{}

    [Route("/tumblr", "POST", Summary = "POST a tumblr news", Notes = "")]
    public class CreateTumblrNews : WebNews, IReturn<WebNews>{}

    [Route("/tumblr/search", "GET", Summary = "GET a list of tumblr news via opensearch", Notes = "")]
    public class SearchTumblrNews {}

}

