using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperStorage.Infrastructure.Persistence.Identity;

namespace SuperStorage.Infrastructure.Persistence.Configuration.Identity;

internal sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("Roles", SuperStorageDbContext.IdentitySchema);

        builder.Property(role => role.Description)
            .HasMaxLength(500);

        builder.Property(role => role.CreatedAt)
            .IsRequired();
    }
}
