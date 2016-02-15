using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using Terradue.WebService.Model;
using System.Web;
using Terradue.OpenSearch.Engine;
using Terradue.Corporate.Controller;
using Terradue.News;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Twitter;
using Terradue.OpenSearch;
using System.Linq;
using ServiceStack.Common.Web;

namespace Terradue.Corporate.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class NewsService : ServiceStack.ServiceInterface.Service {

        public object Get(SearchNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            IOpenSearchResultCollection result = null;
            try{
                context.Open();

                // Load the complete request
                HttpRequest httpRequest = HttpContext.Current.Request;

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;

                Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);

                List<Terradue.OpenSearch.IOpenSearchable> osentities = new List<Terradue.OpenSearch.IOpenSearchable>();

                try{
                    EntityList<Article> articles = new EntityList<Article>(context);
                    articles.Load();
                    osentities.Add(articles);
                }catch(Exception){}

                try{
                    List<TwitterFeed> twitters = TwitterNews.LoadTwitterFeeds(context);
                    foreach(TwitterFeed twitter in twitters) osentities.Add(twitter);
                }catch(Exception){}

                try{
                    EntityList<RssNews> rsss = new EntityList<RssNews>(context);
                    rsss.Load();
                    foreach(RssNews rss in rsss) osentities.Add(rss);
                }catch(Exception){}

                MultiGenericOpenSearchable multiOSE = new MultiGenericOpenSearchable(osentities, ose);

                result = ose.Query(multiOSE, httpRequest.QueryString, type);


                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
                
            return new HttpResult(result.SerializeToString(), result.ContentType);
        }

        public object Get(GetAllNewsFeeds request) {
            List<WebNews> result = new List<WebNews>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                //get internal news
                EntityList<Article> news = new EntityList<Article>(context);
                news.Load();
                foreach(Terradue.Portal.Article f in news){
                    if(f.GetType() == typeof(Article))
                        result.Add(new WebNews(f));
                }

                //get twitter news
                List<TwitterFeed> twitters = TwitterNews.LoadTwitterFeeds(context);
                List<TwitterNews> tweetsfeeds = new List<TwitterNews>();
                foreach(TwitterFeed tweet in twitters) tweetsfeeds.AddRange(TwitterNews.FromFeeds(context, tweet.GetFeeds()));
                foreach(TwitterNews tweetfeed in tweetsfeeds) result.Add(new WebNews(tweetfeed));

                //get rss news
                EntityList<RssNews> rsss = new EntityList<RssNews>(context);
                rsss.Load();
                List<RssNews> rssfeeds = new List<RssNews>();
                foreach(RssNews rss in rsss) rssfeeds.AddRange(rss.GetFeeds());
                foreach(RssNews rssfeed in rssfeeds) result.Add(new WebNews(rssfeed));

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(GetAllNews request) {
            List<WebNews> result = new List<WebNews>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();
                //get internal news
                EntityList<Terradue.Portal.Article> news = new EntityList<Terradue.Portal.Article>(context);
                news.Load();
                foreach(Terradue.Portal.Article f in news) result.Add(new WebNews(f));

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(GetNews request) {
            WebNews result = null;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                result = new WebNews(Terradue.Portal.Article.FromId(context,request.Id));

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Post(CreateNews request) {
            WebNews result = null;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();

                Article article = new Article(context);
                article = request.ToEntity(context, article);
                article.Store();
                result = new WebNews(article);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Put(UpdateNews request) {
            WebNews result = null;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();

                Article article = Article.FromId(context, request.Id);
                article = request.ToEntity(context, article);
                article.Store();
                result = new WebNews(article);

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        public object Delete(DeleteNews request) {
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                Terradue.Portal.Article news = Terradue.Portal.Article.FromId(context,request.Id);
                news.Delete();

                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return true;
        }

    }

    [Route("/news/feeds", "GET", Summary = "GET a list of news feeds", Notes = "")]
    public class GetAllNewsFeeds : IReturn<List<WebNews>>{}
}

