using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using UserManagement.Repository.Sql.Configuration;

namespace UserManagement.Repository.Sql;

// Used by dotnet-ef only; the running application configures the context through AddRepositories.
[ExcludeFromCodeCoverage]
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UserManagementDbContext>
{
    public UserManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=usermanagement;Username=usermanagement;Password=usermanagement";

        var options = new DbContextOptionsBuilder<UserManagementDbContext>().UseUserManagementPostgres(connectionString);
        return new UserManagementDbContext(((DbContextOptionsBuilder<UserManagementDbContext>)options).Options);
    }
}
