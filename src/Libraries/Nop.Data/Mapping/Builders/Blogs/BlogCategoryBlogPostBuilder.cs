using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Blogs;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Blogs
{
    /// <summary>
    /// Represents a product category entity builder
    /// </summary>
    public partial class BlogCategoryBlogPostBuilder : NopEntityBuilder<BlogCategoryBlogPost>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(BlogCategoryBlogPost.CategoryId)).AsInt32().ForeignKey<BlogCategory>()
                .WithColumn(nameof(BlogCategoryBlogPost.BlogId)).AsInt32().ForeignKey<BlogPost>();
        }

        #endregion
    }
}