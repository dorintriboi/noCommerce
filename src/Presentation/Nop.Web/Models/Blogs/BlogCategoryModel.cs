using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Blogs
{
    public partial record BlogCategoryModel : BaseNopEntityModel
    {
        public BlogCategoryModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<BlogOverviewModel>();
            SubCategories = new List<SubCategoryModel>();
            CategoryBreadcrumb = new List<BlogCategoryModel>();
            CatalogProductsModel = new CatalogBlogsModel();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        
        public PictureModel PictureModel { get; set; }

        public bool DisplayCategoryBreadcrumb { get; set; }
        public IList<BlogCategoryModel> CategoryBreadcrumb { get; set; }
        
        public IList<SubCategoryModel> SubCategories { get; set; }

        public IList<BlogOverviewModel> FeaturedProducts { get; set; }

        public CatalogBlogsModel CatalogProductsModel { get; set; }

        #region Nested Classes

        public partial record SubCategoryModel : BaseNopEntityModel
        {
            public SubCategoryModel()
            {
                PictureModel = new PictureModel();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public string Description { get; set; }

            public PictureModel PictureModel { get; set; }
        }

        #endregion
    }
}