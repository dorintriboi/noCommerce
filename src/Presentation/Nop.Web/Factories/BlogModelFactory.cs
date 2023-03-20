using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Services.Blogs;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the blog model factory
    /// </summary>
    public partial class BlogModelFactory : IBlogModelFactory
    {
        #region Fields
        private readonly SeoSettings _seoSettings;
        private readonly BlogSettings _blogSettings;
        private readonly IWebHelper _webHelper;
        private readonly CatalogSettings _catalogSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IBlogService _blogService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPictureService _pictureService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ILocalizationService _localizationService;
        private readonly VendorSettings _vendorSettings;
        private readonly IVideoService _videoService;

        #endregion

        #region Ctor

        public BlogModelFactory(BlogSettings blogSettings,
            CaptchaSettings captchaSettings,
            IWebHelper webHelper,
            SeoSettings seoSettings,
            IVideoService _videoService,
            ILocalizationService localizationService,
            IBlogService blogService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            IPictureService pictureService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            VendorSettings vendorSettings,
            MediaSettings mediaSettings)
        {
            _vendorSettings = vendorSettings;
            _seoSettings = seoSettings;
            _webHelper = webHelper;
            _localizationService = localizationService;
           
            _blogSettings = blogSettings;
            _captchaSettings = captchaSettings;
         
            _blogService = blogService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            _pictureService = pictureService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Prepare blog post model
        /// </summary>
        /// <param name="model">Blog post model</param>
        /// <param name="blogPost">Blog post entity</param>
        /// <param name="prepareComments">Whether to prepare blog comments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrepareBlogPostModelAsync(BlogPostModel model, BlogPost blogPost, bool prepareComments)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (blogPost == null)
                throw new ArgumentNullException(nameof(blogPost));

            model.Id = blogPost.Id;
            model.MetaTitle = blogPost.MetaTitle;
            model.MetaDescription = blogPost.MetaDescription;
            model.MetaKeywords = blogPost.MetaKeywords;
            model.SeName = await _urlRecordService.GetSeNameAsync(blogPost, blogPost.LanguageId, ensureTwoPublishedLanguages: false);
            model.Title = blogPost.Title;
            model.Body = blogPost.Body;
            model.BodyOverview = blogPost.BodyOverview;
            model.AllowComments = blogPost.AllowComments;

            model.PreventNotRegisteredUsersToLeaveComments =
                await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) &&
                !_blogSettings.AllowNotRegisteredUsersToLeaveComments;

            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(blogPost.StartDateUtc ?? blogPost.CreatedOnUtc, DateTimeKind.Utc);
            model.Tags = await _blogService.ParseTagsAsync(blogPost);
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnBlogCommentPage;

            //number of blog comments
            var store = await _storeContext.GetCurrentStoreAsync();
            var storeId = _blogSettings.ShowBlogCommentsPerStore ? store.Id : 0;

            model.NumberOfComments = await _blogService.GetBlogCommentsCountAsync(blogPost, storeId, true);

            if (prepareComments)
            {
                var blogComments = await _blogService.GetAllCommentsAsync(
                    blogPostId: blogPost.Id,
                    approved: true,
                    storeId: storeId);

                foreach (var bc in blogComments)
                {
                    var commentModel = await PrepareBlogPostCommentModelAsync(bc);
                    model.Comments.Add(commentModel);
                }
            }
        }
        public virtual async Task<IEnumerable<BlogOverviewModel>> PrepareBlogOverviewModelsAsync(IEnumerable<BlogPost> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<BlogOverviewModel>();
            foreach (var product in products)
            {
                var model = new BlogOverviewModel
                {
                    Id = product.Id,
                    Name = product.Title,
                    ShortDescription = product.MetaDescription,
                    SeName = await _urlRecordService.GetSeNameAsync(product)
                };
                

                //picture
                if (preparePictureModel)
                {
                    model.PictureModels = await PrepareBlogOverviewPicturesModelAsync(product, productThumbPictureSize);
                }
                

                //reviews
                model.ReviewOverviewModel = await PrepareBlogReviewOverviewModelAsync(product);

                models.Add(model);
            }

            return models;
        }
        protected virtual async Task<BlogReviewOverviewModel> PrepareBlogReviewOverviewModelAsync(BlogPost product)
        {
            BlogReviewOverviewModel productReview = null;
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.BlogReviewsModelKey,
                    product, currentStore);

                productReview = await _staticCacheManager.GetAsync(cacheKey, async () =>
                {
                    var productReviews = await _blogService.GetAllBlogReviewsAsync(productId: product.Id,
                        approved: true, storeId: currentStore.Id);

                    return new BlogReviewOverviewModel
                    {
                        RatingSum = productReviews.Sum(pr => pr.Rating), TotalReviews = productReviews.Count
                    };
                });

                if (productReview != null)
                {
                    productReview.BlogId = product.Id;
                }
            }

            return productReview;
        }
        
         protected virtual async Task<IList<PictureModel>> PrepareBlogOverviewPicturesModelAsync(BlogPost product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = product.Title;
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductOverviewPicturesModelKey, 
                product, pictureSize, true, _catalogSettings.DisplayAllPicturesOnCatalogPages, await _workContext.GetWorkingLanguageAsync(), 
                _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var cachedPictures = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                async Task<PictureModel> preparePictureModelAsync(Picture picture)
                {
                    //we have to keep the url generation order "full size -> preview" because picture can be updated twice
                    //this section of code requires detailed analysis in the future
                    (var fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (var imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                    return new PictureModel
                    {
                        ImageUrl = imageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        //"title" attribute
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                            ? picture.TitleAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                                productName),
                        //"alt" attribute
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                            ? picture.AltAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                                productName)
                    };
                }

                //all pictures
                var pictures = (await _pictureService
                    .GetPicturesByProductIdAsync(product.Id,  _catalogSettings.DisplayAllPicturesOnCatalogPages ? 0 : 1))
                    .DefaultIfEmpty(null);
                var pictureModels = await pictures
                    .SelectAwait(async picture => await preparePictureModelAsync(picture))
                    .ToListAsync();
                return pictureModels;
            });

            return cachedPictures;
        }
         /*public virtual async Task<BlogDetailsModel> PrepareBlogDetailsModelAsync(BlogPost product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //standard properties
            var model = new BlogDetailsModel
            {
                Id = product.Id,
                Name = product.Title,/**/
                /*ShortDescription = product.MetaDescription,
                MetaKeywords =product.MetaKeywords,
                MetaDescription =  product.MetaDescription,
                MetaTitle = product.MetaTitle,
                SeName = await _urlRecordService.GetSeNameAsync(product),
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ShowGtin = _catalogSettings.ShowGtin,
                DisplayDiscontinuedMessage =  _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts
            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && string.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }
            

            var store = await _storeContext.GetCurrentStoreAsync();
            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = await _localizationService.GetLocalizedAsync(store, x => x.Name);
            

            //page sharing
            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the add this link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }

                model.PageShareCode = shareCode;
            }

            
            

            //pictures and videos
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            IList<PictureModel> allPictureModels;
            IList<VideoModel> allVideoModels;
            (model.DefaultPictureModel, allPictureModels, allVideoModels) = await PrepareProductDetailsPictureModelAsync(product, isAssociatedProduct);
            model.PictureModels = allPictureModels;
            model.VideoModels = allVideoModels;

            //price
            model.ProductPrice = await PrepareProductPriceModelAsync(product);

            //'Add to cart' model
            model.AddToCart = await PrepareProductAddToCartModelAsync(product, updatecartitem);
            var customer = await _workContext.GetCurrentCustomerAsync();
            //gift card
            if (product.IsGiftCard)
            {
                model.GiftCard.IsGiftCard = true;
                model.GiftCard.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = await _customerService.GetCustomerFullNameAsync(customer);
                    model.GiftCard.SenderEmail = customer.Email;
                }
                else
                {
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out var giftCardRecipientName, out var giftCardRecipientEmail,
                        out var giftCardSenderName, out var giftCardSenderEmail, out var giftCardMessage);

                    model.GiftCard.RecipientName = giftCardRecipientName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                }
            }

            //product attributes
            model.ProductAttributes = await PrepareProductAttributeModelsAsync(product, updatecartitem);

            //product specifications
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
            }

            //product review overview
            model.ProductReviewOverview = await PrepareProductReviewOverviewModelAsync(product);

            model.ProductReviews = await PrepareProductReviewsModelAsync(product);

            //tier prices
            if (product.HasTierPrices && await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = await PrepareProductTierPriceModelsAsync(product);
            }

            //manufacturers
            model.ProductManufacturers = await PrepareProductManufacturerModelsAsync(product);

            //rental products
            if (product.IsRental)
            {
                model.IsRental = true;
                //set already entered dates attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    model.RentalStartDate = updatecartitem.RentalStartDateUtc;
                    model.RentalEndDate = updatecartitem.RentalEndDateUtc;
                }
            }

            //estimate shipping
            if (_shippingSettings.EstimateShippingProductPageEnabled && !model.IsFreeShipping)
            {
                var wrappedProduct = new ShoppingCartItem
                {
                    StoreId = store.Id,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                var estimateShippingModel = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(new[] { wrappedProduct });

                model.ProductEstimateShipping.ProductId = product.Id;
                model.ProductEstimateShipping.RequestDelay = estimateShippingModel.RequestDelay;
                model.ProductEstimateShipping.Enabled = estimateShippingModel.Enabled;
                model.ProductEstimateShipping.CountryId = estimateShippingModel.CountryId;
                model.ProductEstimateShipping.StateProvinceId = estimateShippingModel.StateProvinceId;
                model.ProductEstimateShipping.ZipPostalCode = estimateShippingModel.ZipPostalCode;
                model.ProductEstimateShipping.UseCity = estimateShippingModel.UseCity;
                model.ProductEstimateShipping.City = estimateShippingModel.City;
                model.ProductEstimateShipping.AvailableCountries = estimateShippingModel.AvailableCountries;
                model.ProductEstimateShipping.AvailableStates = estimateShippingModel.AvailableStates;
            }

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareProductDetailsModelAsync(associatedProduct, null, true));
                }
                model.InStock = model.AssociatedProducts.Any(associatedProduct => associatedProduct.InStock);
            }

            return model;
        }
        #1#*/
         
          protected virtual async Task<(PictureModel pictureModel, IList<PictureModel> allPictureModels, IList<VideoModel> allVideoModels)> PrepareBlogDetailsPictureModelAsync(BlogPost product, bool isAssociatedProduct)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.BlogDetailsPicturesModelKey
                , product, defaultPictureSize, isAssociatedProduct, 
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = product.Title;

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                var defaultPicture = pictures.FirstOrDefault();

                (var fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, !isAssociatedProduct);
                (var imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, !isAssociatedProduct);

                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                        defaultPicture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                    //"alt" attribute
                    AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                        defaultPicture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName)
                };

                //all pictures
                var pictureModels = new List<PictureModel>();
                for (var i = 0; i < pictures.Count; i++ )
                {
                    var picture = pictures[i];

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
                    (var thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });

            var allPictureModels = cachedPictures.PictureModels;
            
            //all videos
            var allvideoModels = new List<VideoModel>();
            var videos = await _videoService.GetVideosByProductIdAsync(product.Id);
            foreach (var video in videos)
            {
                var videoModel = new VideoModel
                {
                    VideoUrl = video.VideoUrl,
                    Allow = _mediaSettings.VideoIframeAllow,
                    Width = _mediaSettings.VideoIframeWidth,
                    Height = _mediaSettings.VideoIframeHeight
                };

                allvideoModels.Add(videoModel);
            }
            return (cachedPictures.DefaultPictureModel, allPictureModels, allvideoModels);
        }

        /// <summary>
        /// Prepare blog post list model
        /// </summary>
        /// <param name="command">Blog paging filtering model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post list model
        /// </returns>
        public virtual async Task<BlogPostListModel> PrepareBlogPostListModelAsync(BlogPagingFilteringModel command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.PageSize <= 0)
                command.PageSize = _blogSettings.PostsPageSize;
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            var dateFrom = command.GetFromMonth();
            var dateTo = command.GetToMonth();

            var language = await _workContext.GetWorkingLanguageAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var blogPosts = string.IsNullOrEmpty(command.Tag)
                ? await _blogService.GetAllBlogPostsAsync(store.Id, language.Id, dateFrom, dateTo, command.PageNumber - 1, command.PageSize)
                : await _blogService.GetAllBlogPostsByTagAsync(store.Id, language.Id, command.Tag, command.PageNumber - 1, command.PageSize);

            var model = new BlogPostListModel
            {
                PagingFilteringContext = { Tag = command.Tag, Month = command.Month },
                WorkingLanguageId = language.Id,
                BlogPosts = await blogPosts.SelectAwait(async blogPost =>
                {
                    var blogPostModel = new BlogPostModel();
                    await PrepareBlogPostModelAsync(blogPostModel, blogPost, false);
                    return blogPostModel;
                }).ToListAsync()
            };
            model.PagingFilteringContext.LoadPagedList(blogPosts);

            return model;
        }

        /// <summary>
        /// Prepare blog post tag list model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog post tag list model
        /// </returns>
        public virtual async Task<BlogPostTagListModel> PrepareBlogPostTagListModelAsync()
        {
            var model = new BlogPostTagListModel();
            var store = await _storeContext.GetCurrentStoreAsync();

            //get tags
            var tags = (await _blogService
                .GetAllBlogPostTagsAsync(store.Id, (await _workContext.GetWorkingLanguageAsync()).Id))
                .OrderByDescending(x => x.BlogPostCount)
                .Take(_blogSettings.NumberOfTags);

            //sorting and setting into the model
            model.Tags.AddRange(tags.OrderBy(x => x.Name).Select(tag => new BlogPostTagModel
            {
                Name = tag.Name,
                BlogPostCount = tag.BlogPostCount
            }));

            return model;
        }

        /// <summary>
        /// Prepare blog post year models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of blog post year model
        /// </returns>
        public virtual async Task<List<BlogPostYearModel>> PrepareBlogPostYearModelAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.BlogMonthsModelKey, currentLanguage, store);
            var cachedModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<BlogPostYearModel>();

                var blogPosts = await _blogService.GetAllBlogPostsAsync(store.Id,
                    currentLanguage.Id);
                if (blogPosts.Any())
                {
                    var months = new SortedDictionary<DateTime, int>();

                    var blogPost = blogPosts[blogPosts.Count - 1];
                    var first = blogPost.StartDateUtc ?? blogPost.CreatedOnUtc;
                    while (DateTime.SpecifyKind(first, DateTimeKind.Utc) <= DateTime.UtcNow.AddMonths(1))
                    {
                        var list = await _blogService.GetPostsByDateAsync(blogPosts, new DateTime(first.Year, first.Month, 1),
                            new DateTime(first.Year, first.Month, 1).AddMonths(1).AddSeconds(-1));
                        if (list.Any())
                        {
                            var date = new DateTime(first.Year, first.Month, 1);
                            months.Add(date, list.Count);
                        }

                        first = first.AddMonths(1);
                    }

                    var current = 0;
                    foreach (var kvp in months)
                    {
                        var date = kvp.Key;
                        var blogPostCount = kvp.Value;
                        if (current == 0)
                            current = date.Year;

                        if (date.Year > current || !model.Any())
                        {
                            var yearModel = new BlogPostYearModel
                            {
                                Year = date.Year
                            };
                            model.Insert(0, yearModel);
                        }

                        model.First().Months.Insert(0, new BlogPostMonthModel
                        {
                            Month = date.Month,
                            BlogPostCount = blogPostCount
                        });

                        current = date.Year;
                    }
                }

                return model;
            });

            return cachedModel;
        }
        
        /// <summary>
        /// Prepare blog comment model
        /// </summary>
        /// <param name="blogComment">Blog comment entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog comment model
        /// </returns>
        public virtual async Task<BlogCommentModel> PrepareBlogPostCommentModelAsync(BlogComment blogComment)
        {
            if (blogComment == null)
                throw new ArgumentNullException(nameof(blogComment));

            var customer = await _customerService.GetCustomerByIdAsync(blogComment.CustomerId);

            var model = new BlogCommentModel
            {
                Id = blogComment.Id,
                CustomerId = blogComment.CustomerId,
                CustomerName = await _customerService.FormatUsernameAsync(customer),
                CommentText = blogComment.CommentText,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(blogComment.CreatedOnUtc, DateTimeKind.Utc),
                AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !await _customerService.IsGuestAsync(customer)
            };

            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                model.CustomerAvatarUrl = await _pictureService.GetPictureUrlAsync(
                    await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                    _mediaSettings.AvatarPictureSize, _customerSettings.DefaultAvatarEnabled, defaultPictureType: PictureType.Avatar);
            }

            return model;
        }

        #endregion
    }
}