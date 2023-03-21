using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial record BlogOverviewModel : BaseNopEntityModel
    {
        public BlogOverviewModel()
        {
            PictureModels = new List<PictureModel>();
            ReviewOverviewModel = new BlogReviewOverviewModel();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string SeName { get; set; }
        
        public BlogType BlogType { get; set; }

        public bool MarkAsNew { get; set; }
        
        public IList<PictureModel> PictureModels { get; set; }
     
        public BlogSpecificationModel BlogSpecificationModel { get; set; }
        public BlogReviewOverviewModel ReviewOverviewModel { get; set; }
        
    }
}