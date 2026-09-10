using Microsoft.EntityFrameworkCore;
using UserManagement.Repository.Sql;

namespace UserManagement.Repository.Sql.Seeding;

public sealed class DatabaseSeeder(UserManagementDbContext db)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var seed = new SeedData();

        // The logs need the users' generated ids, so both saves run in one transaction.
        await db.ExecuteInTransactionAsync(async ct =>
        {
            var users = seed.Users();
            db.Users.AddRange(users);
            await db.SaveChangesAsync(ct);

            db.UserLogs.AddRange(seed.Logs(users));
            return await db.SaveChangesAsync(ct);
        }, cancellationToken);
    }
}
