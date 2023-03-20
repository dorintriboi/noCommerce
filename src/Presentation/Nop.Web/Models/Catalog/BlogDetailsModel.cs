using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Models.Catalog
{
    public partial record BlogDetailsModel : BaseNopEntityModel
    {
        public BlogDetailsModel()
        {
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            VideoModels = new List<VideoModel>();
            GiftCard = new GiftCardModel();
            AddToCart = new AddToCartModel();
            BlogAttributes = new List<BlogAttributeModel>();
            AssociatedBlogs = new List<BlogDetailsModel>();
            VendorModel = new VendorBriefInfoModel();
            BlogBreadcrumb = new BlogDetailsModel.BlogBreadcrumbModel();
            BlogTags = new List<BlogTagModel>();
            BlogSpecificationModel = new BlogSpecificationModel();
            BlogManufacturers = new List<ManufacturerBriefInfoModel>();
            BlogReviewOverview = new BlogReviewOverviewModel();
            BlogReviews = new BlogReviewsModel();
            TierPrices = new List<TierPriceModel>();
        }

        //picture(s)
        public bool DefaultPictureZoomEnabled { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }

        //videos
        public IList<VideoModel> VideoModels { get; set; }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public bool VisibleIndividually { get; set; }

        public BlogType BlogType { get; set; }

        public bool ShowManufacturerPartNumber { get; set; }
        public string ManufacturerPartNumber { get; set; }

        public bool ShowGtin { get; set; }
        public string Gtin { get; set; }

        public bool ShowVendor { get; set; }
        public VendorBriefInfoModel VendorModel { get; set; }

        public bool HasSampleDownload { get; set; }

        public GiftCardModel GiftCard { get; set; }

        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool FreeShippingNotificationEnabled { get; set; }
        public string DeliveryDate { get; set; }

        public bool IsRental { get; set; }
        public DateTime? RentalStartDate { get; set; }
        public DateTime? RentalEndDate { get; set; }

        public DateTime? AvailableEndDate { get; set; }

        public ManageInventoryMethod ManageInventoryMethod { get; set; }

        public string StockAvailability { get; set; }

        public bool DisplayBackInStockSubscription { get; set; }

        public bool EmailAFriendEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }

        public string PageShareCode { get; set; }

        public AddToCartModel AddToCart { get; set; }

        public BlogBreadcrumbModel BlogBreadcrumb { get; set; }

        public IList<BlogTagModel> BlogTags { get; set; }

        public IList<BlogAttributeModel> BlogAttributes { get; set; }

        public BlogSpecificationModel BlogSpecificationModel { get; set; }

        public IList<ManufacturerBriefInfoModel> BlogManufacturers { get; set; }

        public BlogReviewOverviewModel BlogReviewOverview { get; set; }

        public BlogReviewsModel BlogReviews { get; set; }

        public BlogEstimateShippingModel BlogEstimateShipping { get; set; }

        public IList<TierPriceModel> TierPrices { get; set; }

        //a list of associated products. For example, "Grouped" products could have several child "simple" products
        public IList<BlogDetailsModel> AssociatedBlogs { get; set; }

        public bool DisplayDiscontinuedMessage { get; set; }

        public string CurrentStoreName { get; set; }

        public bool InStock { get; set; }

        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }

        #region Nested Classes

        public partial record BlogBreadcrumbModel : BaseNopModel
        {

            public bool Enabled { get; set; }
            public int BlogId { get; set; }
            public string BlogName { get; set; }
            public string BlogSeName { get; set; }
            public IList<BlogCategorySimpleModel> BlogCategoryBreadcrumb { get; set; }
        }

        public partial record AddToCartModel : BaseNopModel
        {
            public AddToCartModel()
            {
                AllowedQuantities = new List<SelectListItem>();
            }
            public int ProductId { get; set; }

            //qty
            [NopResourceDisplayName("Products.Qty")]
            public int EnteredQuantity { get; set; }
            public string MinimumQuantityNotification { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }

            //price entered by customers
            [NopResourceDisplayName("Products.EnterProductPrice")]
            public bool CustomerEntersPrice { get; set; }
            [NopResourceDisplayName("Products.EnterProductPrice")]
            public decimal CustomerEnteredPrice { get; set; }
            public string CustomerEnteredPriceRange { get; set; }

            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }

            //rental
            public bool IsRental { get; set; }

            //pre-order
            public bool AvailableForPreOrder { get; set; }
            public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
            public string PreOrderAvailabilityStartDateTimeUserTime { get; set; }

            //updating existing shopping cart or wishlist item?
            public int UpdatedShoppingCartItemId { get; set; }
            public ShoppingCartType? UpdateShoppingCartItemType { get; set; }
        }

      

        public partial record GiftCardModel : BaseNopModel
        {
            public bool IsGiftCard { get; set; }

            [NopResourceDisplayName("Products.GiftCard.RecipientName")]
            public string RecipientName { get; set; }

            [NopResourceDisplayName("Products.GiftCard.RecipientEmail")]
            [DataType(DataType.EmailAddress)]
            public string RecipientEmail { get; set; }

            [NopResourceDisplayName("Products.GiftCard.SenderName")]
            public string SenderName { get; set; }

            [NopResourceDisplayName("Products.GiftCard.SenderEmail")]
            [DataType(DataType.EmailAddress)]
            public string SenderEmail { get; set; }

            [NopResourceDisplayName("Products.GiftCard.Message")]
            public string Message { get; set; }

            public GiftCardType GiftCardType { get; set; }
        }

        public partial record TierPriceModel : BaseNopModel
        {
            public string Price { get; set; }
            public decimal PriceValue { get; set; }

            public int Quantity { get; set; }
        }

        public partial record BlogAttributeModel : BaseNopEntityModel
        {
            public BlogAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<BlogAttributeValueModel>();
            }

            public int BlogId { get; set; }

            public int BlogAttributeId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }
            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            /// A value indicating whether this attribute depends on some other attribute
            /// </summary>
            public bool HasCondition { get; set; }

            /// <summary>
            /// Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<BlogAttributeValueModel> Values { get; set; }
        }

        public partial record BlogAttributeValueModel : BaseNopEntityModel
        {
            public BlogAttributeValueModel()
            {
                ImageSquaresPictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string ColorSquaresRgb { get; set; }

            //picture model is used with "image square" attribute type
            public PictureModel ImageSquaresPictureModel { get; set; }

            public string PriceAdjustment { get; set; }

            public bool PriceAdjustmentUsePercentage { get; set; }

            public decimal PriceAdjustmentValue { get; set; }

            public bool IsPreSelected { get; set; }

            //product picture ID (associated to this value)
            public int PictureId { get; set; }

            public bool CustomerEntersQty { get; set; }

            public int Quantity { get; set; }
        }

        public partial record BlogEstimateShippingModel : EstimateShippingModel
        {
            public int BlogId { get; set; }
        }

        #endregion
    }
}