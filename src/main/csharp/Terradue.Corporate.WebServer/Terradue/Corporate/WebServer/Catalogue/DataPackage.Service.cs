using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using Terradue.OpenSearch;
using Terradue.Portal;
using Terradue.WebService.Model;
using Terradue.Corporate.WebServer.Common;
using Terradue.Corporate.Controller;



namespace Terradue.Corporate.WebServer
{

    [Api("Tep-Quickwin Terradue webserver")]
	[Restrict(EndpointAttributes.InSecure | EndpointAttributes.InternalNetworkAccess | EndpointAttributes.Json,
	          EndpointAttributes.Secure   | EndpointAttributes.External | EndpointAttributes.Json)]
	public class DataPackageService : ServiceStack.ServiceInterface.Service
	{		
		/// <summary>
		/// Get the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Get(GetAllDataPackages request)
		{
			//Get all requests from database
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
			List<WebDataPackageTep> result = new List<WebDataPackageTep> ();
			try{
				context.Open();
                EntityList<DataPackage> tmpList = new EntityList<DataPackage>(context);
                tmpList.Load();
                foreach(DataPackage a in tmpList)
                    result.Add(new WebDataPackageTep(a));
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}

			return result;
		}

		/// <summary>
		/// Get the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Get(GetDataPackage request)
		{
			//Get all requests from database
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
			WebDataPackageTep result;
			try{
				context.Open();
                DataPackage tmp = DataPackage.FromId(context,request.Id);
                result = new WebDataPackageTep(tmp);
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}

			return result;
		}

		/// <summary>
		/// Post the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
        public object Post(CreateDataPackageTep request)
		{
			//Get all requests from database
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
			WebDataPackageTep result;
			try{
				context.Open();
                DataPackage tmp = new DataPackage(context);
                tmp = (DataPackage)request.ToEntity(context, tmp);
				tmp.Store();
                result = new WebDataPackageTep(tmp);
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}

			return result;
		}

		/// <summary>
		/// Put the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
        public object Put(UpdateDataPackageTep request)
		{
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
			WebDataPackageTep result;
			try{
				context.Open();
                DataPackage tmp = DataPackage.FromId(context, request.Id);
                tmp = (DataPackage)request.ToEntity(context, tmp);
				tmp.Store();
                result = new WebDataPackageTep(tmp);
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}

			return result;
		}

		/// <summary>
		/// Delete the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Delete(DeleteDataPackage request)
		{
            IfyWebContext context = new T2CorporateWebContext(PagePrivileges.UserView);
			try{
				context.Open();
                DataPackage tmp = DataPackage.FromId(context,request.Id);
				tmp.Delete();
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}

            return Get(new GetAllDataPackages());
		}

        /// <summary>
        /// Post the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        public object Post(AddItemToDataPackage request)
        {
            //Get all requests from database
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
            WebDataPackageTep result;
            try{
                context.Open();
                DataPackage tmp = DataPackage.FromId(context,request.DpId);
                RemoteResource tmp2 = request.ToEntity(context);
                tmp.AddResourceItem(tmp2);
                result = new WebDataPackageTep(tmp);
                context.Close ();
            }catch(Exception e) {
                context.Close ();
                throw e;
            }

            return result;
        }

		/// <summary>
		/// Delete the specified request.
		/// </summary>
		/// <param name="request">Request.</param>
		public object Delete(RemoveItemFromDataPackage request)
		{
            IfyWebContext context = T2CorporateWebContext.GetWebContext(PagePrivileges.UserView);
			WebDataPackageTep result;
			try{
				context.Open();
                RemoteResource tmp = RemoteResource.FromId(context,request.Id);
				tmp.Delete();
                DataPackage dp = DataPackage.FromId(context,request.DpId);
                result = new WebDataPackageTep(dp);
				context.Close ();
			}catch(Exception e) {
				context.Close ();
                throw e;
			}

			return result;
		}

	}
}

