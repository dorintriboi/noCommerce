using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Category template service interface
    /// </summary>
    public partial interface IBlogCategoryTemplateService
    {
        /// <summary>
        /// Delete category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteCategoryTemplateAsync(BlogCategoryTemplate categoryTemplate);

        /// <summary>
        /// Gets all category templates
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category templates
        /// </returns>
        Task<IList<BlogCategoryTemplate>> GetAllCategoryTemplatesAsync();

        /// <summary>
        /// Gets a category template
        /// </summary>
        /// <param name="categoryTemplateId">Category template identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category template
        /// </returns>
        Task<BlogCategoryTemplate> GetCategoryTemplateByIdAsync(int categoryTemplateId);

        /// <summary>
        /// Inserts category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertCategoryTemplateAsync(BlogCategoryTemplate categoryTemplate);

        /// <summary>
        /// Updates the category template
        /// </summary>
        /// <param name="categoryTemplate">Category template</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateCategoryTemplateAsync(BlogCategoryTemplate categoryTemplate);
    }
}