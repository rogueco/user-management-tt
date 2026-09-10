using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Users;

namespace UserManagement.Repository.Sql.Configurations;

internal sealed class UserLogConfiguration : IEntityTypeConfiguration<UserLog>
{
    public void Configure(EntityTypeBuilder<UserLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Forename).HasMaxLength(50);
        builder.Property(l => l.Surname).HasMaxLength(50);
        builder.Property(l => l.Email).HasMaxLength(254);
        builder.Property(l => l.Action).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Changes).HasColumnType("jsonb");
        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.Timestamp).IsDescending();
    }
}
