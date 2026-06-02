using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OP.GATEWAY.Models;

namespace OP.GATEWAY.Data
{
    public class GatewayDbContextFactory : IDesignTimeDbContextFactory<GatewayDbContext>
    {
        public GatewayDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../OP.GATEWAY");

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();
            string connectionString = config.GetConnectionString("MySqlLogDb")
                ?? throw new InvalidOperationException("Connection string 'MySqlLogDb' not found. Ensure appsettings.json contains it and SetBasePath is correct.");

            optionsBuilder.UseMySQL(connectionString);
            return new GatewayDbContext(optionsBuilder.Options);
        }
    }

    public class GatewayDbContext : DbContext
    {
        public DbSet<ApiUser> ApiUsers { get; set; }
        public DbSet<AisApiLog> AisApiLogs { get; set; }

        public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options) { }
    }
}
