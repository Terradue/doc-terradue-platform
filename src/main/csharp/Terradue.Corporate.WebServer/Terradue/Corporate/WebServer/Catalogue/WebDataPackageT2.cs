using System;
using ServiceStack.ServiceHost;
using System.Collections.Generic;
using Terradue.WebService.Model;
using Terradue.Portal;
using Terradue.Corporate.Controller;

namespace Terradue.Corporate.WebServer {

    [Route("/datapackage", "POST", Summary = "POST a datapackage", Notes = "Add a new datapackage in database")]
    public class CreateDataPackageT2 : WebDataPackageT2, IReturn<WebDataPackageT2>{}

    [Route("/datapackage", "PUT", Summary = "PUT a datapackage", Notes = "Update a datapackage in database")]
    public class UpdateDataPackageT2 : WebDataPackageT2, IReturn<WebDataPackageT2>{}

    public class WebDataPackageT2 : Terradue.WebService.Model.WebDataPackage {

        [ApiMember(Name="AccessKey", Description = "Remote resource AccessKey", ParameterType = "path", DataType = "string", IsRequired = false)]
        public string AccessKey { get; set; }

        public WebDataPackageT2() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.TepQW.WebServer.WebDataPackageTep"/> class.
        /// </summary>
        /// <param name="entity">Entity.</param>
        public WebDataPackageT2(DataPackage entity)
        {
            this.Id = entity.Id;
            this.Name = entity.Name;
            this.Identifier = entity.Identifier;
            this.IsDefault = entity.IsDefault;
            this.AccessKey = entity.AccessKey;
            this.Items = new List<WebDataPackageItem>();
            foreach (RemoteResource item in entity.Resources) this.Items.Add(new WebDataPackageItem(item));

        }

        /// <summary>
        /// To the entity.
        /// </summary>
        /// <returns>The entity.</returns>
        /// <param name="context">Context.</param>
        public new DataPackage ToEntity(IfyContext context, DataPackage input){
            DataPackage result = (input == null ? new DataPackage(context) : input);

            result.Name = this.Name;
            result.Identifier = this.Identifier;
            result.IsDefault = this.IsDefault;
            result.Items = new EntityList<RemoteResource>(context);
            result.Items.Template.ResourceSet = result;
            if (this.Items != null) {
                foreach (WebDataPackageItem item in this.Items) {
                    RemoteResource res = (item.Id == 0) ? new RemoteResource(context) : RemoteResource.FromId(context, item.Id);
                    res = item.ToEntity(context, res);
                    result.Items.Include(res);
                }
            }

            return result;
        }
    }
}

