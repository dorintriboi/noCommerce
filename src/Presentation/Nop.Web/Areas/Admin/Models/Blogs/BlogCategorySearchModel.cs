﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Blogs
{
    /// <summary>
    /// Represents a category search model
    /// </summary>
    public partial record BlogCategorySearchModel : BaseSearchModel
    {
        #region Ctor

        public BlogCategorySearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.BlogCategories.List.SearchCategoryName")]
        public string SearchBlogCategoryName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BlogCategories.List.SearchPublished")]
        public int SearchPublishedId { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BlogCategories.List.SearchStore")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}