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
            MapLocation(modelBuilder.Entity<Location>());
            MapMesajSablon(modelBuilder.Entity<MesajSablon>());
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
        protected virtual void MapLocation(EntityTypeBuilder<Location> config)
        {
            config.ToTable("LOCATION");
            config.HasKey(t => t.Id);
            config.Property(t => t.Id).HasColumnName("Id");
            config.Property(t => t.Name).HasColumnName("Name").IsUnicode(false).IsFixedLength(false).HasMaxLength(50);
            config.Property(t => t.Latitude).HasColumnName("Latitude");
            config.Property(t => t.Country).HasColumnName("Country");
            config.Property(t => t.Longitude).HasColumnName("Longitude");
        }
        protected virtual void MapMesajSablon(EntityTypeBuilder<MesajSablon> config)
        {
            config.ToTable("MESAJSABLON");
            config.HasKey(t => t.Id);
            config.Property(t => t.Id).HasColumnName("Id");
            config.Property(t => t.SablonTip).HasColumnName("SABLON_TIP");
            config.Property(t => t.Modul).HasColumnName("MODUL").IsUnicode(true).IsFixedLength(false).HasMaxLength(100);
            config.Property(t => t.SablonKod).HasColumnName("SABLON_KOD").IsUnicode(true).IsFixedLength(false).HasMaxLength(100).IsRequired();
            config.Property(t => t.Konu).HasColumnName("KONU").IsUnicode(true).IsFixedLength(false).HasMaxLength(1000);
            config.Property(t => t.Mesaj).HasColumnName("MESAJ").IsUnicode(false).IsFixedLength(false).HasMaxLength(2147483647);
            config.Property(t => t.Aciklama).HasColumnName("ACIKLAMA").IsUnicode(true).IsFixedLength(false).HasMaxLength(2000);
        }
        protected virtual void MapCustomer(EntityTypeBuilder<Customer> config)
        {
            config.ToTable("CUSTOMER");
            config.HasKey(t => t.Id);
            config.Property(t => t.Id).HasColumnName("Id");
            config.Property(t => t.KimlikNo).HasColumnName("KIMLIK_NO");
            config.Property(t => t.Ad).HasColumnName("AD").IsUnicode(false).IsFixedLength(false).HasMaxLength(500).IsRequired();
            config.Property(t => t.CustomerTuru).HasColumnName("CUSTOMER_TURU").IsUnicode(false).IsFixedLength(false).HasMaxLength(1).IsRequired();
            config.Property(t => t.Adres).HasColumnName("ADRES").IsUnicode(false).IsFixedLength(false).HasMaxLength(200);
            config.Property(t => t.TelAlanKod).HasColumnName("TEL_ALAN_KOD");
            config.Property(t => t.TelNo).HasColumnName("TEL_NO").IsUnicode(false).IsFixedLength(false).HasMaxLength(7);
            config.Property(t => t.FaxAlanKod).HasColumnName("FAX_ALAN_KOD");
            config.Property(t => t.FaxNo).HasColumnName("FAX_NO").IsUnicode(false).IsFixedLength(false).HasMaxLength(7);
            config.Property(t => t.AnaCustomerKod).HasColumnName("ANA_Customer_KOD");
            config.Property(t => t.AktifEh).HasColumnName("AKTIF_EH").IsUnicode(false).IsFixedLength(false).HasMaxLength(1).IsRequired();
            config.Property(t => t.Notlar).HasColumnName("NOTLAR").IsUnicode(false).IsFixedLength(false).HasMaxLength(250);
            config.Property(t => t.Seviye).HasColumnName("SEVIYE");
            config.Property(t => t.YetkiliCustomerKod).HasColumnName("YETKILI_CUSTOMER_KOD");
            config.Property(t => t.YetkiliAdSoyad).HasColumnName("YETKILI_AD_SOYAD").IsUnicode(false).IsFixedLength(false).HasMaxLength(110);
            config.Property(t => t.YetkiliUnvani).HasColumnName("YETKILI_UNVANI").IsUnicode(false).IsFixedLength(false).HasMaxLength(50);
            config.Property(t => t.YetkiliTelNo).HasColumnName("YETKILI_TEL_NO").IsUnicode(false).IsFixedLength(false).HasMaxLength(25);
            config.Property(t => t.EPosta).HasColumnName("E_POSTA").IsUnicode(false).IsFixedLength(false).HasMaxLength(100);
            config.Property(t => t.KisaAd).HasColumnName("KISA_AD").IsUnicode(false).IsFixedLength(false).HasMaxLength(20);
            config.HasOne(t => t.AnaCustomer).WithMany(t => t.Customers).HasForeignKey(t => t.AnaCustomerKod);

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
