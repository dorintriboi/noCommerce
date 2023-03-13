using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Blogs
{
    public partial record BlogCategoryNavigationModel : BaseNopModel
    {
        public BlogCategoryNavigationModel()
        {
            Categories = new List<BlogCategorySimpleModel>();
        }

        public int CurrentCategoryId { get; set; }
        public List<BlogCategorySimpleModel> Categories { get; set; }

        #region Nested classes

        public partial record CategoryLineModel : BaseNopModel
        {
            public int CurrentCategoryId { get; set; }
            public BlogCategorySimpleModel Category { get; set; }
        }

        #endregion
    }
}