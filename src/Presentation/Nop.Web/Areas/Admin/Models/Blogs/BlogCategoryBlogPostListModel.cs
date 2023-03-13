using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Blogs
{
    /// <summary>
    /// Represents a category product list model
    /// </summary>
    public partial record BlogCategoryBlogPostListModel : BasePagedListModel<BlogCategoryBlogPostModel>
    {
    }
}