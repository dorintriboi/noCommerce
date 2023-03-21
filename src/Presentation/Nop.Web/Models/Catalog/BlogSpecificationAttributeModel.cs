using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a specification attribute model
    /// </summary>
    public partial record BlogSpecificationAttributeModel : BaseNopEntityModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the values
        /// </summary>
        public IList<BlogSpecificationAttributeValueModel> Values { get; set; }

        #endregion

        #region Ctor

        public BlogSpecificationAttributeModel()
        {
            Values = new List<BlogSpecificationAttributeValueModel>();
        }

        #endregion
    }
}