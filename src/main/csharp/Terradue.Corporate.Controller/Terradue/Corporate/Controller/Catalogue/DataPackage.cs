using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Web;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Schema;
using Terradue.Portal;

/*! 
 * \defgroup modules_contest_DataPackage Data Package
 * @{
 * \ingroup modules_contest
 * \section sec1_datapackage Definition
 * Data Package is an entity containing a set of data defined by a user. Data are defined by an opensearch URL. A Data Package thus contain a list of URLs.\n
 * A Data Package is associated to a user.
 * @}
 */

namespace Terradue.Corporate.Controller {

	/// <summary>
	/// Data package.
	/// </summary>
    /// \ingroup modules_contest_DataPackage
    [EntityTable(null, EntityTableConfiguration.Custom, Storage = EntityTableStorage.Above)]
	public class DataPackage : RemoteResourceSet {

		/// <summary>
		/// Gets or sets the items.
		/// </summary>
		/// <value>The items.</value>
        public EntityList<RemoteResource> Items { get;	set; }

		/// <summary>
		/// Gets or sets the resources.
		/// </summary>
		/// <value>The resources.</value>
        public override EntityList<RemoteResource> Resources {
			get {
                EntityList<RemoteResource> result = new EntityList<RemoteResource>(context);
                result.Template.ResourceSet = this;
                if (Items == null) LoadItems();
                foreach (RemoteResource item in Items)
                    result.Include(item);
				return result;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Terradue.Contest.DataPackage"/> class.
		/// </summary>
		/// <param name="context">Context.</param>
		public DataPackage(IfyContext context) : base(context) {}

		/// <summary>
		/// Froms the identifier.
		/// </summary>
		/// <returns>The identifier.</returns>
		/// <param name="context">Context.</param>
		/// <param name="identifier">Identifier.</param>
        public static new DataPackage FromIdentifier(IfyContext context, string identifier) {
			DataPackage result = new DataPackage(context);
			result.Identifier = identifier;
			try {
				result.Load();
			} catch(Exception e) {
				throw e;
			}
			return result;
		}

		/// <summary>
		/// Froms the identifier.
		/// </summary>
		/// <returns>The identifier.</returns>
		/// <param name="context">Context.</param>
		/// <param name="id">Identifier.</param>
		public static new DataPackage FromId(IfyContext context, int id) {
			DataPackage result = new DataPackage(context);
			result.Id = id;
			try {
				result.Load();
			} catch(Exception e) {
				throw e;
			}
			return result;
		}
               
		/// <summary>
		/// Adds the resource item.
		/// </summary>
		/// <param name="item">Item.</param>
        public void AddResourceItem(RemoteResource item) {
            item.Store();
            Items.Include(item);
		}

        /// <summary>
        /// Reads the information of an item from the database.
        /// </summary>
        public override void Load(){
            base.Load();
            LoadItems();
        }

        /// <summary>
        /// Loads the items.
        /// </summary>
        public void LoadItems(){
            Items = new EntityList<RemoteResource>(context);
            Items.Template.ResourceSet = this;
            Items.Load();
        }

        /// <summary>
        /// Writes the item to the database.
        /// </summary>
        public override void Store() {
            context.StartTransaction ();
            try{
                if (this.Id==0) this.AccessKey = Guid.NewGuid().ToString();
                base.Store();
                Resources.StoreExactly();
                LoadItems();
                context.Commit();
            }catch(Exception e){
                context.Rollback();
                throw e;
            }
        }

		/// <summary>
		/// Allows the user.
		/// </summary>
		/// <param name="usrId">Usr identifier.</param>
        public void AllowUser(int usrId) {
			String sql = String.Format ("INSERT IGNORE INTO resourceset_priv (id_resourceset, id_usr) VALUES ({0},{1});",this.Id, usrId);
			context.Execute (sql);
		}

		/// <summary>
		/// Removes the user.
		/// </summary>
		/// <param name="usrId">Usr identifier.</param>
        public void RemoveUser(int usrId) {
			String sql = String.Format ("DELETE FROM resourceset_priv WHERE id_resourceset={0} AND id_usr={1};",this.Id, usrId);
			context.Execute (sql);

			//\todo: Problem if user need to access data package from another contest
		}

        public void SetOpenSearchEngine(OpenSearchEngine ose) {
            this.ose = ose;
        }

        /// <summary>
        /// Gets the local open search description.
        /// </summary>
        /// <returns>The local open search description.</returns>
        public OpenSearchDescription GetLocalOpenSearchDescription() {
            OpenSearchDescription osd = base.GetOpenSearchDescription();

            OpenSearchDescriptionUrl urld = osd.Url[0];
            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>(osd.Url);

            UriBuilder urlb = new UriBuilder(urld.Template);
            NameValueCollection query = HttpUtility.ParseQueryString(urlb.Query);

            query.Set("format", "json");
            urlb.Query = query.ToString();
            OpenSearchDescriptionUrl url = new OpenSearchDescriptionUrl("application/json",urlb.ToString(),"search");
            urls.Add(url);
            osd.Url = urls.ToArray();

            return osd;
        }

        public override IOpenSearchable[] GetOpenSearchableArray() {
            List<UrlBasedOpenSearchable> osResources = new List<UrlBasedOpenSearchable>(Resources.Count);

            foreach (RemoteResource res in Resources) {
                osResources.Add(new UrlBasedOpenSearchable(context,  new OpenSearchUrl(res.Location), ose));
            }

            return osResources.ToArray();
        }

        #region IOpenSearchable implementation

        public OpenSearchUrl GetSearchBaseUrl() {
            return new OpenSearchUrl (string.Format("{0}/datapackage/{1}/search", context.BaseUrl, this.Identifier));
        }

        #endregion
            
	}
}

