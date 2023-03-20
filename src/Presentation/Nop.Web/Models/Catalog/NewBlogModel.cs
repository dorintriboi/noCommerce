using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a new products model
    /// </summary>
    public partial record NewBlogsModel : BaseNopModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the catalog products model
        /// </summary>
        public CatalogBlogsModel CatalogBlogsModel { get; set; }

        #endregion

        #region Ctor

        public NewBlogsModel()
        {
            CatalogBlogsModel = new CatalogBlogsModel();
        }

        #endregion
    }
}