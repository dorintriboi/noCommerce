using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.Blogs
{
    /// <summary>
    /// Blog service
    /// </summary>
    public partial class BlogService : IBlogService
    {
        #region Fields
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly ISearchPluginManager _searchPluginManager;
        private readonly ILanguageService _languageService;
        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<BlogCategoryBlogPost> _productCategoryRepository;
        private readonly IRepository<BlogCategory> _categoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<BlogCategoryBlogPostTagMapping> _productTagMappingRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<BlogReview> _blogReviewRepository;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public BlogService(
            ICustomerService customerService,
            IRepository<BlogReview> blogReviewRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<BlogCategoryBlogPostTagMapping> productTagMappingRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<BlogCategory> categoryRepository,
            IRepository<BlogCategoryBlogPost> productCategoryRepository,
            CatalogSettings catalogSettings,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            ISearchPluginManager searchPluginManager,
            ILanguageService languageService,
            IWorkContext workContext,
            IRepository<BlogComment> blogCommentRepository,
            IRepository<BlogPost> blogPostRepository,
            IStaticCacheManager staticCacheManager,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IStoreMappingService storeMappingService)
        {
            _customerService = customerService;
            _blogReviewRepository = blogReviewRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productTagRepository = productTagRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _manufacturerRepository = manufacturerRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _categoryRepository = categoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _catalogSettings = catalogSettings;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _searchPluginManager = searchPluginManager;
            _languageService = languageService;
            _workContext = workContext;
            _blogCommentRepository = blogCommentRepository;
            _blogPostRepository = blogPostRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        #region Blog posts

        /// <summary>
        /// Deletes a blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteBlogPostAsync(BlogPost blogPost)
        {
            await _blogPostRepository.DeleteAsync(blogPost);
        }
        public virtual async Task<IPagedList<BlogPost>> GetBlogsMarkedAsNewAsync(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _blogPostRepository.Table
                where  !p.Deleted 
                select p;

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();

            query = query.OrderByDescending(p => p.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
        
        public virtual async Task<IList<BlogPost>> GetBlogCategoryFeaturedBlogsAsync(int categoryId, int storeId = 0)
        {
            IList<BlogPost> featuredProducts = new List<BlogPost>();

            if (categoryId == 0)
                return featuredProducts;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.BlogCategoryFeaturedBlogsIdsKey, categoryId, customerRoleIds, storeId);

            var featuredProductIds = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from p in _blogPostRepository.Table
                    join pc in _productCategoryRepository.Table on p.Id equals pc.BlogId
                    where  !p.Deleted  &&
                           pc.IsFeaturedProduct && categoryId == pc.CategoryId
                    select p;

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);
                
                featuredProducts = query.ToList();

                return featuredProducts.Select(p => p.Id).ToList();
            });

            if (featuredProducts.Count == 0 && featuredProductIds.Count > 0)
                featuredProducts = await _blogPostRepository.GetByIdsAsync(featuredProductIds, cache => default, false);

            return featuredProducts;
        }

        public virtual async Task<int> GetNumberOfBlogsInBlogCategoryAsync(IList<int> categoryIds = null, int storeId = 0)
        {
            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            var query = _blogPostRepository.Table.Where(p=>  !p.Deleted);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            //category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                query = from p in query
                    join pc in _productCategoryRepository.Table on p.Id equals pc.BlogId
                    where categoryIds.Contains(pc.CategoryId)
                    select p;
            }

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(NopCatalogDefaults.CategoryProductsNumberCacheKey, customerRoleIds, storeId, categoryIds);

            //only distinct products
            return await _staticCacheManager.GetAsync(cacheKey, () => query.Select(p => p.Id).Count());
        }

        /// <summary>
        /// Gets a blog post
        /// </summary>
        /// <param name="blogPostId">Blog post identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post
        /// </returns>
        public virtual async Task<BlogPost> GetBlogPostByIdAsync(int blogPostId)
        {
            return await _blogPostRepository.GetByIdAsync(blogPostId, cache => default);
        }
        
         public virtual async Task<IPagedList<BlogReview>> GetAllBlogReviewsAsync(int customerId = 0, bool? approved = null,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, int storeId = 0, int productId = 0, int vendorId = 0, bool showHidden = false,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var productReviews = await _blogReviewRepository.GetAllPagedAsync(async query =>
            {
                if (!showHidden)
                {
                    var productsQuery = _blogPostRepository.Table;

                    //apply store mapping constraints
                    productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

                    //apply ACL constraints
                    var customer = await _workContext.GetCurrentCustomerAsync();

                    query = query.Where(review => productsQuery.Any(product => product.Id == review.BlogId));
                }

                if (approved.HasValue)
                    query = query.Where(pr => pr.IsApproved == approved);
                if (customerId > 0)
                    query = query.Where(pr => pr.CustomerId == customerId);
                if (fromUtc.HasValue)
                    query = query.Where(pr => fromUtc.Value <= pr.CreatedOnUtc);
                if (toUtc.HasValue)
                    query = query.Where(pr => toUtc.Value >= pr.CreatedOnUtc);
                if (!string.IsNullOrEmpty(message))
                    query = query.Where(pr => pr.Title.Contains(message) || pr.ReviewText.Contains(message));
                if (storeId > 0)
                    query = query.Where(pr => pr.StoreId == storeId);
                if (productId > 0)
                    query = query.Where(pr => pr.BlogId == productId);

                query = from productReview in query
                        join blog in _blogPostRepository.Table on productReview.BlogId equals blog.Id
                        where
                            !blog.Deleted
                        select productReview;

                query = _catalogSettings.ProductReviewsSortByCreatedDateAscending
                    ? query.OrderBy(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id)
                    : query.OrderByDescending(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id);

                return query;
            }, pageIndex, pageSize);

            return productReviews;
        }

        public async Task<IList<BlogPost>> GetBlogsByIdsAsync(int[] blogIds)
        {
            return await _blogPostRepository.GetByIdsAsync(blogIds, cache => default, false);
        }

        public virtual async Task<IPagedList<BlogPost>> SearchBlogsAsync(
               int pageIndex = 0,
               int pageSize = int.MaxValue,
               IList<int> categoryIds = null,
               IList<int> manufacturerIds = null,
               int storeId = 0,
               int vendorId = 0,
               int warehouseId = 0,
               ProductType? productType = null,
               bool visibleIndividuallyOnly = false,
               bool excludeFeaturedProducts = false,
               decimal? priceMin = null,
               decimal? priceMax = null,
               int productTagId = 0,
               string keywords = null,
               bool searchDescriptions = false,
               bool searchManufacturerPartNumber = true,
               bool searchSku = true,
               bool searchProductTags = false,
               int languageId = 0,
               IList<SpecificationAttributeOption> filteredSpecOptions = null,
               BlogSortingEnum orderBy = BlogSortingEnum.Position,
               bool showHidden = false,
               bool? overridePublished = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _blogPostRepository.Table;
            
            if (!showHidden)
            {
                //apply store mapping constraints
                productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

                //apply ACL constraints
                var customer = await _workContext.GetCurrentCustomerAsync();
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);
                IQueryable<int> productsByKeywords;

                var customer = await _workContext.GetCurrentCustomerAsync();
                var activeSearchProvider = await _searchPluginManager.LoadPrimaryPluginAsync(customer, storeId);

                if (activeSearchProvider is not null)
                {
                    productsByKeywords = (await activeSearchProvider.SearchProductsAsync(keywords, searchLocalizedValue)).AsQueryable();
                }
                else
                {
                    productsByKeywords =
                        from p in _blogPostRepository.Table
                        where p.Title.Contains(keywords)
                        select p.Id;

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                            from lp in _localizedPropertyRepository.Table
                            let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                            lp.LocaleValue.Contains(keywords)
                            let checkShortDesc = searchDescriptions &&
                                            lp.LocaleKey == nameof(Product.ShortDescription) &&
                                            lp.LocaleValue.Contains(keywords)
                            where
                                lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                            select lp.EntityId);
                    }
                }

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                //search by category name if admin allows
                if (_catalogSettings.AllowCustomersToSearchWithCategoryName)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pc in _productCategoryRepository.Table
                        join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                        where c.Name.Contains(keywords)
                        select pc.BlogId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pc in _productCategoryRepository.Table
                        join lp in _localizedPropertyRepository.Table on pc.CategoryId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(Category) &&
                              lp.LocaleKey == nameof(Category.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pc.BlogId);
                    }
                }

                //search by manufacturer name if admin allows
                if (_catalogSettings.AllowCustomersToSearchWithManufacturerName)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pm in _productManufacturerRepository.Table
                        join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                        where m.Name.Contains(keywords)
                        select pm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pm in _productManufacturerRepository.Table
                        join lp in _localizedPropertyRepository.Table on pm.ManufacturerId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(Manufacturer) &&
                              lp.LocaleKey == nameof(Manufacturer.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pm.ProductId);
                    }
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.BlogTagId equals pt.Id
                        where pt.Name.Contains(keywords)
                        select pptm.BlogId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.BlogTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pptm.BlogId);
                    }
                }

                productsQuery =
                    from p in productsQuery
                    join pbk in productsByKeywords on p.Id equals pbk
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.BlogId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Title
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Title
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.BlogId
                    where ptm.BlogTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="title">Filter by blog post title</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog posts
        /// </returns>
        public virtual async Task<IPagedList<BlogPost>> GetAllBlogPostsAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null)
        {
            return await _blogPostRepository.GetAllPagedAsync(async query =>
            {
                if (dateFrom.HasValue)
                    query = query.Where(b => dateFrom.Value <= (b.StartDateUtc ?? b.CreatedOnUtc));

                if (dateTo.HasValue)
                    query = query.Where(b => dateTo.Value >= (b.StartDateUtc ?? b.CreatedOnUtc));

                if (languageId > 0)
                    query = query.Where(b => languageId == b.LanguageId);

                if (!string.IsNullOrEmpty(title))
                    query = query.Where(b => b.Title.Contains(title));

                if (!showHidden)
                {
                    query = query.Where(b => !b.StartDateUtc.HasValue || b.StartDateUtc <= DateTime.UtcNow);
                    query = query.Where(b => !b.EndDateUtc.HasValue || b.EndDateUtc >= DateTime.UtcNow );

                    //apply store mapping constraints
                    query = await _storeMappingService.ApplyStoreMapping(query, storeId);
                }

                query = query.Where(b => !b.Deleted);
                query = query.OrderByDescending(b => b.StartDateUtc ?? b.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all blog posts</param>
        /// <param name="tag">Tag</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog posts
        /// </returns>
        public virtual async Task<IPagedList<BlogPost>> GetAllBlogPostsByTagAsync(int storeId = 0,
            int languageId = 0, string tag = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            tag = tag.Trim();

            //we load all records and only then filter them by tag
            var blogPostsAll = await GetAllBlogPostsAsync(storeId: storeId, languageId: languageId, showHidden: showHidden);
            var taggedBlogPosts = new List<BlogPost>();
            foreach (var blogPost in blogPostsAll)
            {
                var tags = await ParseTagsAsync(blogPost);
                if (!string.IsNullOrEmpty(tags.FirstOrDefault(t => t.Equals(tag, StringComparison.InvariantCultureIgnoreCase))))
                    taggedBlogPosts.Add(blogPost);
            }

            //server-side paging
            var result = new PagedList<BlogPost>(taggedBlogPosts, pageIndex, pageSize);
            return result;
        }

        /// <summary>
        /// Gets all blog post tags
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all blog posts</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post tags
        /// </returns>
        public virtual async Task<IList<BlogPostTag>> GetAllBlogPostTagsAsync(int storeId, int languageId, bool showHidden = false)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.BlogTagsCacheKey, languageId, storeId, showHidden);

            var blogPostTags = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var rezBlogPostTags = new List<BlogPostTag>();

                var blogPosts = await GetAllBlogPostsAsync(storeId, languageId, showHidden: showHidden);

                foreach (var blogPost in blogPosts)
                {
                    var tags = await ParseTagsAsync(blogPost);
                    foreach (var tag in tags)
                    {
                        var foundBlogPostTag = rezBlogPostTags.Find(bpt =>
                            bpt.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                        if (foundBlogPostTag == null)
                        {
                            foundBlogPostTag = new BlogPostTag
                            {
                                Name = tag,
                                BlogPostCount = 1
                            };
                            rezBlogPostTags.Add(foundBlogPostTag);
                        }
                        else
                            foundBlogPostTag.BlogPostCount++;
                    }
                }

                return rezBlogPostTags;
            });

            return blogPostTags;
        }

        /// <summary>
        /// Inserts a blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertBlogPostAsync(BlogPost blogPost)
        {
            await _blogPostRepository.InsertAsync(blogPost);
        }

        public async Task<BlogPost> GetBlogByIdAsync(int productId)
        {
            return await _blogPostRepository.GetByIdAsync(productId, cache => default);
        }
        public virtual bool BlogIsAvailable(BlogPost product, DateTime? dateTime = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            dateTime ??= DateTime.UtcNow;
            
            return true;
        }

        /// <summary>
        /// Updates the blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateBlogPostAsync(BlogPost blogPost)
        {
            await _blogPostRepository.UpdateAsync(blogPost);
        }

        /// <summary>
        /// Returns all posts published between the two dates.
        /// </summary>
        /// <param name="blogPosts">Source</param>
        /// <param name="dateFrom">Date from</param>
        /// <param name="dateTo">Date to</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the filtered posts
        /// </returns>
        public virtual async Task<IList<BlogPost>> GetPostsByDateAsync(IList<BlogPost> blogPosts, DateTime dateFrom, DateTime dateTo)
        {
            if (blogPosts == null)
                throw new ArgumentNullException(nameof(blogPosts));

            var rez = await blogPosts
                .Where(p => dateFrom.Date <= (p.StartDateUtc ?? p.CreatedOnUtc) && (p.StartDateUtc ?? p.CreatedOnUtc).Date <= dateTo)
                .ToListAsync();

            return rez;
        }

        /// <summary>
        /// Parse tags
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ags
        /// </returns>
        public virtual async Task<IList<string>> ParseTagsAsync(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException(nameof(blogPost));

            if (blogPost.Tags == null)
                return new List<string>();

            var tags = await blogPost.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrEmpty(tag))
                .ToListAsync();

            return tags;
        }

        /// <summary>
        /// Get a value indicating whether a blog post is available now (availability dates)
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <param name="dateTime">Datetime to check; pass null to use current date</param>
        /// <returns>Result</returns>
        public virtual bool BlogPostIsAvailable(BlogPost blogPost, DateTime? dateTime = null)
        {
            if (blogPost == null)
                throw new ArgumentNullException(nameof(blogPost));

            if (blogPost.StartDateUtc.HasValue && blogPost.StartDateUtc.Value >= (dateTime ?? DateTime.UtcNow))
                return false;

            if (blogPost.EndDateUtc.HasValue && blogPost.EndDateUtc.Value <= (dateTime ?? DateTime.UtcNow))
                return false;

            return true;
        }

        #endregion

        #region Blog comments

        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="blogPostId">Blog post ID; 0 or null to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item creation to; null to load all records</param>
        /// <param name="commentText">Search comment text; null to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the comments
        /// </returns>
        public virtual async Task<IList<BlogComment>> GetAllCommentsAsync(int customerId = 0, int storeId = 0, int? blogPostId = null,
            bool? approved = null, DateTime? fromUtc = null, DateTime? toUtc = null, string commentText = null)
        {
            return await _blogCommentRepository.GetAllAsync(query =>
            {
                if (approved.HasValue)
                    query = query.Where(comment => comment.IsApproved == approved);

                if (blogPostId > 0)
                    query = query.Where(comment => comment.BlogPostId == blogPostId);

                if (customerId > 0)
                    query = query.Where(comment => comment.CustomerId == customerId);

                if (storeId > 0)
                    query = query.Where(comment => comment.StoreId == storeId);

                if (fromUtc.HasValue)
                    query = query.Where(comment => fromUtc.Value <= comment.CreatedOnUtc);

                if (toUtc.HasValue)
                    query = query.Where(comment => toUtc.Value >= comment.CreatedOnUtc);

                if (!string.IsNullOrEmpty(commentText))
                    query = query.Where(c => c.CommentText.Contains(commentText));

                query = query.OrderBy(comment => comment.CreatedOnUtc);

                return query;
            });
        }

        /// <summary>
        /// Gets a blog comment
        /// </summary>
        /// <param name="blogCommentId">Blog comment identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog comment
        /// </returns>
        public virtual async Task<BlogComment> GetBlogCommentByIdAsync(int blogCommentId)
        {
            return await _blogCommentRepository.GetByIdAsync(blogCommentId, cache => default);
        }

        /// <summary>
        /// Get blog comments by identifiers
        /// </summary>
        /// <param name="commentIds">Blog comment identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog comments
        /// </returns>
        public virtual async Task<IList<BlogComment>> GetBlogCommentsByIdsAsync(int[] commentIds)
        {
            return await _blogCommentRepository.GetByIdsAsync(commentIds);
        }

        /// <summary>
        /// Get the count of blog comments
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="isApproved">A value indicating whether to count only approved or not approved comments; pass null to get number of all comments</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of blog comments
        /// </returns>
        public virtual async Task<int> GetBlogCommentsCountAsync(BlogPost blogPost, int storeId = 0, bool? isApproved = null)
        {
            var query = _blogCommentRepository.Table.Where(comment => comment.BlogPostId == blogPost.Id);

            if (storeId > 0)
                query = query.Where(comment => comment.StoreId == storeId);

            if (isApproved.HasValue)
                query = query.Where(comment => comment.IsApproved == isApproved.Value);

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopBlogsDefaults.BlogCommentsNumberCacheKey, blogPost, storeId, isApproved);

            return await _staticCacheManager.GetAsync(cacheKey, async () => await AsyncIQueryableExtensions.CountAsync(query));
        }

        /// <summary>
        /// Deletes a blog comment
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteBlogCommentAsync(BlogComment blogComment)
        {
            await _blogCommentRepository.DeleteAsync(blogComment);
        }

        /// <summary>
        /// Deletes blog comments
        /// </summary>
        /// <param name="blogComments">Blog comments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteBlogCommentsAsync(IList<BlogComment> blogComments)
        {
            await _blogCommentRepository.DeleteAsync(blogComments);
        }

        /// <summary>
        /// Inserts a blog comment
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertBlogCommentAsync(BlogComment blogComment)
        {
            await _blogCommentRepository.InsertAsync(blogComment);
        }

        /// <summary>
        /// Update a blog comment
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateBlogCommentAsync(BlogComment blogComment)
        {
            await _blogCommentRepository.UpdateAsync(blogComment);
        }

        #endregion

        #endregion
    }
}