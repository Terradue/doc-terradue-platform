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
using Terradue.Portal;
using Terradue.WebService.Model;
using Terradue.Corporate.WebServer.Common;

namespace Terradue.Corporate.WebServer {
    [Api("Tep-QuickWin Terradue webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class RssNewsService : ServiceStack.ServiceInterface.Service {

        [AddHeader(ContentType="application/atom+xml")]
        public object Get(SearchRssNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            IOpenSearchResult result = null;
            try{
                context.Open();

                // Load the complete request
                HttpRequest httpRequest = HttpContext.Current.Request;

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;

                Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);

                EntityList<RssNews> rss = new EntityList<RssNews>(context);
                rss.Load();

                MultiGenericOpenSearchable multiOSE = new MultiGenericOpenSearchable(rss.Cast<IOpenSearchable>().ToList(), ose);

                result = ose.Query(multiOSE, httpRequest.QueryString, type);

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return new HttpResult(result.Result.SerializeToString(), result.Result.ContentType);
        }

        public object Get(GetRssNewsFeeds request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            List<WebNews> result = new List<WebNews>();
            try{
                context.Open();

                EntityList<RssNews> rsss = new EntityList<RssNews>(context);
                rsss.Load();

                List<RssNews> rssfeeds = new List<RssNews>();
                foreach(RssNews rss in rsss) rssfeeds.AddRange(rss.GetFeeds());
                foreach(RssNews rssfeed in rssfeeds) result.Add(new WebNews(rssfeed));

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

        public object Get(GetAllRssNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            List<WebNews> result = new List<WebNews>();
            try{
                context.Open();

                EntityList<RssNews> articles = new EntityList<RssNews>(context);
                articles.Load();
                foreach(RssNews article in articles) result.Add(new WebNews(article));

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

        public object Post(CreateRssNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            WebNews result = null;
            try{
                context.Open();

                RssNews article = new RssNews(context);
                article = (RssNews)request.ToEntity(context, article);
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

    [Route("/rss", "GET", Summary = "GET a list of rss news", Notes = "")]
    public class GetAllRssNews : IReturn<List<WebNews>>{}

    [Route("/rss/feeds", "GET", Summary = "GET a list of rss news feeds", Notes = "")]
    public class GetRssNewsFeeds : IReturn<List<WebNews>>{}

    [Route("/rss", "POST", Summary = "POST a rss news", Notes = "")]
    public class CreateRssNews : WebNews, IReturn<WebNews>{}

    [Route("/rss/search", "GET", Summary = "GET a list of rss news via opensearch", Notes = "")]
    public class SearchRssNews {}
}

