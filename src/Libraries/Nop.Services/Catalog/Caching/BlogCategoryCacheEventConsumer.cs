using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;
using Nop.Services.Discounts;

namespace Nop.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a category cache event consumer
    /// </summary>
    public partial class BlogCategoryCacheEventConsumer : CacheEventConsumer<BlogCategory>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(BlogCategory entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(NopCatalogDefaults.BlogCategoriesByParentCategoryPrefix, entity);
            await RemoveByPrefixAsync(NopCatalogDefaults.BlogCategoriesByParentCategoryPrefix, entity.ParentCategoryId);
            await RemoveByPrefixAsync(NopCatalogDefaults.BlogCategoriesChildIdsPrefix, entity);
            await RemoveByPrefixAsync(NopCatalogDefaults.BlogCategoriesChildIdsPrefix, entity.ParentCategoryId);
            await RemoveByPrefixAsync(NopCatalogDefaults.BlogCategoriesHomepagePrefix);
            await RemoveByPrefixAsync(NopCatalogDefaults.BlogCategoryBreadcrumbPrefix);
            await RemoveByPrefixAsync(NopDiscountDefaults.BlogCategoryIdsPrefix);

            if (entityEventType == EntityEventType.Delete)
                await RemoveAsync(NopCatalogDefaults.SpecificationAttributeOptionsByCategoryCacheKey, entity);

            await RemoveAsync(NopDiscountDefaults.AppliedDiscountsCacheKey, nameof(BlogCategory), entity);

            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}