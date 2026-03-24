using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Fintex.EntityFrameworkCore;

public static class FintexDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<FintexDbContext> builder, string connectionString)
    {
        builder.UseNpgsql(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<FintexDbContext> builder, DbConnection connection)
    {
        builder.UseNpgsql(connection);
    }
}
