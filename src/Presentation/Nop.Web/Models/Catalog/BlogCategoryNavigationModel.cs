using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Blogs;

namespace Nop.Web.Models.Catalog
{
    public partial record BlogCategoryNavigationModel : BaseNopModel
    {
        public BlogCategoryNavigationModel()
        {
            BlogCategories = new List<BlogCategorySimpleModel>();
        }

        public int CurrentBlogCategoryId { get; set; }
        public List<BlogCategorySimpleModel> BlogCategories { get; set; }

        #region Nested classes

        public partial record BlogCategoryLineModel : BaseNopModel
        {
            public int CurrentBlogCategoryId { get; set; }
            public BlogCategorySimpleModel BlogCategory { get; set; }
        }

        #endregion
    }
}