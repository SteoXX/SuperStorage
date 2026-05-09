using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperStorage.Domain.Products;
using SuperStorage.Infrastructure.Persistence;

namespace SuperStorage.Infrastructure.Persistence.Configuration.Products;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", WmsDbContext.WmsSchema);

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Sku)
            .HasConversion(
                sku => sku.Value,
                value => Sku.Create(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(Product.DescriptionMaxLength);

        builder.Property(product => product.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.HasIndex(product => product.Name);
    }
}
