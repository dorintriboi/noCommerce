using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a product specification model
    /// </summary>
    public partial record BlogSpecificationModel : BaseNopModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the grouped specification attribute models
        /// </summary>
        public IList<BlogSpecificationAttributeGroupModel> Groups { get; set; }

        #endregion

        #region Ctor

        public BlogSpecificationModel()
        {
            Groups = new List<BlogSpecificationAttributeGroupModel>();
        }

        #endregion
    }
}