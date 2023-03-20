using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a grouped product specification attribute model
    /// </summary>
    public partial record BlogSpecificationAttributeGroupModel : BaseNopEntityModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the specification attribute group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute group attributes
        /// </summary>
        public IList<BlogSpecificationAttributeModel> Attributes { get; set; }

        #endregion

        #region Ctor

        public BlogSpecificationAttributeGroupModel()
        {
            Attributes = new List<BlogSpecificationAttributeModel>();
        }

        #endregion
    }
}