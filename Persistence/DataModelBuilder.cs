using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public partial class DataModelBuilder
    {
        public virtual void BuildModel(ModelBuilder modelBuilder)
        {
            MapProduct(modelBuilder.Entity<Product>());
            MapMarket(modelBuilder.Entity<Market>());
        }
        protected virtual void MapProduct(EntityTypeBuilder<Product> config)
        {
            config.ToTable("PRODUCT");
            config.HasKey(t => t.Id);
            config.Property(t => t.Id).HasColumnName("Id");
            config.Property(t => t.Description).HasColumnName("Description").IsUnicode(false).IsFixedLength(false).HasMaxLength(50).IsRequired();
            config.Property(t => t.Price).HasColumnName("Price");
            config.Property(t => t.Price).HasColumnName("Name");
            // config.HasOne(t => t.Market).WithMany(t => t.Products).HasForeignKey(t => t.MarketId);
        }
        protected virtual void MapMarket(EntityTypeBuilder<Market> config)
        {
            config.ToTable("MARKET");
            config.HasKey(t => t.Id);
            config.Property(t => t.Id).HasColumnName("Id");
            config.Property(t => t.BrandName).HasColumnName("BrandName").IsUnicode(false).IsFixedLength(false).HasMaxLength(50).IsRequired();
            config.Property(t => t.Geolocation).HasColumnName("Geolocation");
        }
    }
    public static partial class ModelBuilderExtensions
    {
        private static readonly string READONLY_ANNOTATION = "custom:readonly";

        internal static EntityTypeBuilder<TEntity> IsReadOnly<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class
        {
            builder.HasAnnotation(READONLY_ANNOTATION, true);
            return builder;
        }

        public static bool IsReadOnly(this IEntityType entity)
        {
            var annotation = entity.FindAnnotation(READONLY_ANNOTATION);
            return annotation != null && (bool)annotation.Value;
        }
    }
}
