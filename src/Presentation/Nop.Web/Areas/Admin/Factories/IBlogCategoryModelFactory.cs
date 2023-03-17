using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Blogs;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the category model factory
    /// </summary>
    public partial interface IBlogCategoryModelFactory
    {
        /// <summary>
        /// Prepare category search model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category search model
        /// </returns>
        Task<BlogCategorySearchModel> PrepareCategorySearchModelAsync(BlogCategorySearchModel searchModel);

        /// <summary>
        /// Prepare paged category list model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category list model
        /// </returns>
        Task<BlogCategoryListModel> PrepareCategoryListModelAsync(BlogCategorySearchModel searchModel);

        /// <summary>
        /// Prepare category model
        /// </summary>
        /// <param name="model">Category model</param>
        /// <param name="category">Category</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category model
        /// </returns>
        Task<BlogCategoryModel> PrepareCategoryModelAsync(BlogCategoryModel model, BlogCategory category, bool excludeProperties = false);

        /// <summary>
        /// Prepare paged category product list model
        /// </summary>
        /// <param name="searchModel">Category product search model</param>
        /// <param name="category">Category</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category product list model
        /// </returns>
        Task<BlogCategoryBlogPostListModel> PrepareCategoryProductListModelAsync(BlogCategoryBlogPostSearchModel searchModel, BlogCategory category);

        /// <summary>
        /// Prepare product search model to add to the category
        /// </summary>
        /// <param name="searchModel">Product search model to add to the category</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product search model to add to the category
        /// </returns>
        Task<AddBlogToCategorySearchModel> PrepareAddBlogToCategorySearchModelAsync(AddBlogToCategorySearchModel searchModel);

        /// <summary>
        /// Prepare paged product list model to add to the category
        /// </summary>
        /// <param name="searchModel">Product search model to add to the category</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model to add to the category
        /// </returns>
        Task<AddBlogToCategoryListModel> PrepareAddProductToCategoryListModelAsync(AddBlogToCategorySearchModel searchModel);
    }
}