using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using Terradue.Corporate.Controller;
using Terradue.Corporate.WebServer.Common;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;
using Terradue.Portal;
using Terradue.WebService.Model;

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

                context.LogInfo(this, string.Format("/news/search GET query='{0}'", httpRequest.QueryString));

                OpenSearchEngine ose = MasterCatalogue.OpenSearchEngine;

                Type type = OpenSearchFactory.ResolveTypeFromRequest(httpRequest, ose);

                EntityList<Article> articles = new EntityList<Article>(context);
                articles.Load();
                var identifier = articles.Identifier;

                List<Article> tmp = articles.GetItemsAsList();
                tmp.Sort();
                tmp.Reverse();

                articles = new EntityList<Article>(context);
                articles.Identifier = identifier;
                foreach (Article a in tmp) articles.Include(a);

                result = ose.Query(articles, httpRequest.QueryString, type);

                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }
                
            return new HttpResult(result.SerializeToString(), result.ContentType);
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
                context.LogError(this, e.Message + " - " + e.StackTrace);
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
                context.LogError(this, e.Message + " - " + e.StackTrace);
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
                context.LogError(this, e.Message + " - " + e.StackTrace);
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
                context.LogError(this, e.Message + " - " + e.StackTrace);
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
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return true;
        }

        public object Get(GetAllNewsTags request) {
            List<WebKeyValue> result = new List<WebKeyValue>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            try {
                context.Open();

                EntityList<Article> news = new EntityList<Article>(context);
                news.Load();

                var pairs = new Dictionary<string,int>();
                foreach(var n in news){
                    foreach(var tag in n.Tags.Split(",".ToCharArray())){
                        if(pairs.ContainsKey(tag)){
                            pairs[tag]++;
                        } else {
                            pairs.Add(tag, 1);
                        }
                    }
                }

                foreach (KeyValuePair<string, int> kv in pairs.OrderByDescending(key => key.Value)){
                    result.Add(new WebKeyValue(kv.Key, kv.Value + ""));
                }

                context.Close();
            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }
            return result;
        }

        public object Get(DescriptionNews request){
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.EverybodyView);
            IOpenSearchResultCollection result = null;
            try {
                context.Open();

                EntityList<Article> articles = new EntityList<Article>(context);
                var osd = articles.GetOpenSearchDescription();

                context.Close();

                return new HttpResult(osd,"application/opensearchdescription+xml");

            } catch (Exception e) {
                context.LogError(this, e.Message + " - " + e.StackTrace);
                context.Close();
                throw e;
            }

            return new HttpResult(result.SerializeToString(), result.ContentType);
        }

    }

    [Route("/news/feeds", "GET", Summary = "GET a list of news feeds", Notes = "")]
    public class GetAllNewsFeeds : IReturn<List<WebNews>>{}

    [Route("/news/tags", "GET", Summary = "GET a list of news tags", Notes = "")]
    public class GetAllNewsTags : IReturn<List<WebKeyValue>>{}
}

