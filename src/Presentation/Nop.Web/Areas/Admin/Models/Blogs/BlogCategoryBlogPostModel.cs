using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs
{
    /// <summary>
    /// Represents a category product model
    /// </summary>
    public partial record BlogCategoryBlogPostModel : BaseNopEntityModel
    {
        #region Properties

        public int CategoryId { get; set; }

        public int BlogId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.Product")]
        public string BlogName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.IsFeaturedProduct")]
        public bool IsFeaturedProduct { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Categories.Products.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}