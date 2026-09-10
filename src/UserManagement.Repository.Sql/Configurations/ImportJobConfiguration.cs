using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Imports;

namespace UserManagement.Repository.Sql.Configurations;

internal sealed class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.FileName).HasMaxLength(255);
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(j => j.Errors).HasColumnType("jsonb");
        builder.HasIndex(j => j.CreatedAt).IsDescending();
    }
}
