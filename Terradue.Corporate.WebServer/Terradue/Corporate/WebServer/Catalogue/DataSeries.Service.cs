using System;
using ServiceStack.ServiceHost;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.WebServer.Common;
using System.Collections.Generic;

namespace Terradue.Corporates.WebServer {
    [Api("Terradue Corporate webserver")]
    [Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
              EndpointAttributes.Secure | EndpointAttributes.External | EndpointAttributes.Json)]
    public class DataSeriesService : ServiceStack.ServiceInterface.Service {
        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(GetAllSeries request) {
            List<WebSeries> result = new List<WebSeries>();

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                EntityList<Series> series = new EntityList<Series>(context);
                series.Load();
                foreach(Series s in series) result.Add(new WebSeries(s));
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Get the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Get(GetSerie request) {
            WebSeries result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            try {
                context.Open();
                Series serie = Series.FromId(context, request.Id);
                result = new WebSeries(serie);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(CreateSerie request) {
            WebSeries result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            try {
                context.Open();
                Series serie = request.ToEntity(context);
                serie.Store();
                serie.StoreGlobalPrivileges();
                result = new WebSeries(serie);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// Put the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Put(UpdateSerie request) {
            WebSeries result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            try {
                context.Open();
                Series serie = request.ToEntity(context);
                serie.Store();
                result = new WebSeries(serie);
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return result;
        }


        /// <summary>
        /// Delete the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Delete(DeleteSerie request) {
            WebSeries result;

            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.AdminOnly);
            try {
                context.Open();
                Series serie = Series.FromId(context, request.Id);
                serie.Delete();
                context.Close();
            } catch (Exception e) {
                context.Close();
                throw e;
            }
            return Get(new GetAllSeries());
        }
    }
}

