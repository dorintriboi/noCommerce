using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Catalog
{
    public partial record BlogReviewOverviewModel : BaseNopModel
    {
        public int BlogId { get; set; }

        public int RatingSum { get; set; }

        public int TotalReviews { get; set; }

        public bool AllowCustomerReviews { get; set; }

        public bool CanAddNewReview { get; set; }
    }

    public partial record BlogReviewsModel : BaseNopModel
    {
        public BlogReviewsModel()
        {
            Items = new List<BlogReviewModel>();
            AddBlogReview = new AddBlogReviewModel();
            ReviewTypeList = new List<ReviewTypeModel>();
            AddAdditionalBlogReviewList = new List<AddBlogReviewReviewTypeMappingModel>();
        }

        public int BlogId { get; set; }

        public IList<BlogReviewModel> Items { get; set; }

        public AddBlogReviewModel AddBlogReview { get; set; }

        public IList<ReviewTypeModel> ReviewTypeList { get; set; }

        public IList<AddBlogReviewReviewTypeMappingModel> AddAdditionalBlogReviewList { get; set; }
    }
    
    public partial record BlogReviewModel : BaseNopEntityModel
    {
        public BlogReviewModel()
        {
            AdditionalBlogReviewList = new List<BlogReviewReviewTypeMappingModel>();
        }

        public int CustomerId { get; set; }

        public string CustomerAvatarUrl { get; set; }

        public string CustomerName { get; set; }

        public bool AllowViewingProfiles { get; set; }

        public string Title { get; set; }

        public string ReviewText { get; set; }

        public string ReplyText { get; set; }

        public int Rating { get; set; }

        public string WrittenOnStr { get; set; }

        public BlogReviewHelpfulnessModel Helpfulness { get; set; }

        public IList<BlogReviewReviewTypeMappingModel> AdditionalBlogReviewList { get; set; }
    }

    public partial record BlogReviewHelpfulnessModel : BaseNopModel
    {
        public int BlogReviewId { get; set; }

        public int HelpfulYesTotal { get; set; }

        public int HelpfulNoTotal { get; set; }
    }

    public partial record AddBlogReviewModel : BaseNopModel
    {
        [NopResourceDisplayName("Reviews.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Reviews.Fields.ReviewText")]
        public string ReviewText { get; set; }

        [NopResourceDisplayName("Reviews.Fields.Rating")]
        public int Rating { get; set; }

        public bool DisplayCaptcha { get; set; }

        public bool CanCurrentCustomerLeaveReview { get; set; }

        public bool CanAddNewReview { get; set; }
    }

    public partial record AddBlogReviewReviewTypeMappingModel : BaseNopEntityModel
    {
        public int BlogReviewId { get; set; }

        public int ReviewTypeId { get; set; }

        public int Rating { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsRequired { get; set; }
    }

    public partial record BlogReviewReviewTypeMappingModel : BaseNopEntityModel
    {
        public int BlogReviewId { get; set; }

        public int ReviewTypeId { get; set; }

        public int Rating { get; set; }

        public string Name { get; set; }

        public bool VisibleToAllCustomers { get; set; }
    }
}