using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Blogs;

namespace Nop.Data.Mapping.Builders.Blogs
{
    /// <summary>
    /// Represents a category entity builder
    /// </summary>
    public partial class BlogCategoryBuilder : NopEntityBuilder<BlogCategory>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(BlogCategory.Name)).AsString(400).NotNullable()
                .WithColumn(nameof(BlogCategory.MetaKeywords)).AsString(400).Nullable()
                .WithColumn(nameof(BlogCategory.MetaTitle)).AsString(400).Nullable()
                .WithColumn(nameof(BlogCategory.PageSizeOptions)).AsString(200).Nullable();
        }

        #endregion
    }
}