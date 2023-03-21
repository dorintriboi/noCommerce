namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents the product URL structure type enum
    /// </summary>
    public enum BlogUrlStructureType
    {
        /// <summary>
        /// Product only (e.g. '/product-seo-name')
        /// </summary>
        Blog = 0,

        /// <summary>
        /// Category (the most nested), then product (e.g. '/category-seo-name/product-seo-name')
        /// </summary>
        BlogCategory = 10,

        /// <summary>
        /// Manufacturer, then product (e.g. '/manufacturer-seo-name/product-seo-name')
        /// </summary>
        ManufacturerBlog = 20
    }
}