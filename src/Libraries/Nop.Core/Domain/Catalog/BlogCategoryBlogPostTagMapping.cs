namespace Nop.Core.Domain.Catalog;

public class BlogCategoryBlogPostTagMapping : BaseEntity
{
    public int BlogId { get; set; }

    /// <summary>
    /// Gets or sets the product tag identifier
    /// </summary>
    public int BlogTagId { get; set; }
}