using Microsoft.AspNetCore.Identity;

namespace SuperStorage.Infrastructure.Persistence.Identity;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
