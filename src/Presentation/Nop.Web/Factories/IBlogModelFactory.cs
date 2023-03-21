using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Orders;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the interface of the blog model factory
    /// </summary>
    public partial interface IBlogModelFactory
    {
        /// <summary>
        /// Prepare blog post model
        /// </summary>
        /// <param name="model">Blog post model</param>
        /// <param name="blogPost">Blog post entity</param>
        /// <param name="prepareComments">Whether to prepare blog comments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PrepareBlogPostModelAsync(BlogPostModel model, BlogPost blogPost, bool prepareComments);
        
        Task<IEnumerable<BlogOverviewModel>> PrepareBlogOverviewModelsAsync(IEnumerable<BlogPost> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false);

        /*Task<BlogDetailsModel> PrepareBlogDetailsModelAsync(BlogPost product, ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false);*/
        /// <summary>
        /// Prepare blog post list model
        /// </summary>
        /// <param name="command">Blog paging filtering model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post list model
        /// </returns>
        Task<BlogPostListModel> PrepareBlogPostListModelAsync(BlogPagingFilteringModel command);

        /// <summary>
        /// Prepare blog post tag list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post tag list model
        /// </returns>
        Task<BlogPostTagListModel> PrepareBlogPostTagListModelAsync();

        /// <summary>
        /// Prepare blog post year models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of blog post year model
        /// </returns>
        Task<List<BlogPostYearModel>> PrepareBlogPostYearModelAsync();

        /// <summary>
        /// Prepare blog comment model
        /// </summary>
        /// <param name="blogComment">Blog comment entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog comment model
        /// </returns>
        Task<BlogCommentModel> PrepareBlogPostCommentModelAsync(BlogComment blogComment);
    }
}