using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Users;

namespace UserManagement.Repository.Sql.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Forename).HasMaxLength(50);
        builder.Property(u => u.Surname).HasMaxLength(50);
        builder.Property(u => u.Email).HasMaxLength(254);
        builder.Ignore(u => u.Details);
    }
}
