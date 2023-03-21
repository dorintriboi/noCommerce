using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Blogs
{
    /// <summary>
    /// Represents a product model to add to the category
    /// </summary>
    public partial record AddBlogToCategoryModel : BaseNopModel
    {
        #region Ctor

        public AddBlogToCategoryModel()
        {
            SelectedBlogsIds = new List<int>();
        }
        #endregion

        #region Properties

        public int CategoryId { get; set; }

        public IList<int> SelectedBlogsIds { get; set; }

        #endregion
    }
}