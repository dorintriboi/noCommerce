using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Builders.Blogs
{
    /// <summary>
    /// Represents a category template entity builder
    /// </summary>
    public partial class BlogCategoryTemplateBuilder : NopEntityBuilder<BlogCategoryTemplate>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table) 
        {
            table
                .WithColumn(nameof(BlogCategoryTemplate.Name)).AsString(400).NotNullable()
                .WithColumn(nameof(BlogCategoryTemplate.ViewPath)).AsString(400).NotNullable();
        }

        #endregion
    }
}