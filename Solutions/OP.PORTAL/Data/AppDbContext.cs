using Microsoft.EntityFrameworkCore;
using OP.PORTAL.Locales;
using OP.PORTAL.Models;
using System.Text.Json;
using static MudBlazor.Colors;
using static System.Net.WebRequestMethods;

namespace OP.PORTAL.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _http;
        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor http)
            : base(options) { _http = http; }

        public DbSet<OvmcRequest> OvmcRequests => Set<OvmcRequest>();

        public DbSet<OvmcPayment> OvmcPayments => Set<OvmcPayment>();

        public DbSet<Sponsor> Sponsors => Set<Sponsor>();

        public DbSet<GeneralMaster> GeneralMasters => Set<GeneralMaster>();

        public DbSet<SmsRequest> SmsRequests => Set<SmsRequest>();

        public DbSet<AppResource> AppResources => Set<AppResource>();

        public DbSet<OvmcAuditLog> OvmcAuditLogs => Set<OvmcAuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppResource>()
                .HasIndex(u => u.ResourceKey)
                .IsUnique();

            modelBuilder.Entity<OvmcPayment>()
                .HasIndex(u => u.OrderNo)
                .IsUnique();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await AddAuditLogs();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task AddAuditLogs()
        {
            ChangeTracker.DetectChanges();

            IList<OvmcAuditLog> auditLogs = new List<OvmcAuditLog>();

            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted);
            
            string? _ipAddress = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var ignoredColumns = new[] { "CreatedDate", "ModifiedDate", "LastModifiedBy" };

            foreach (var entry in entries)
            {
                if (entry.Entity is OvmcAuditLog)
                    continue;

                string? _userName = entry.Properties
                    .FirstOrDefault(p => p.Metadata.Name == "LastModifiedBy")
                    ?.CurrentValue?.ToString() ?? "Anonymous_2";

                var audit = new OvmcAuditLog
                {
                    TableName = entry.Metadata.GetTableName()!,
                    Action = entry.State.ToString().ToUpper(),
                    ChangedBy = _userName,
                    IpAddress = _ipAddress,
                    ChangedOn = DateTime.Now
                };

                audit.RecordId = entry.Properties
                    .First(p => p.Metadata.IsPrimaryKey())
                    .CurrentValue?.ToString() ?? "";

                if (entry.State == EntityState.Added)
                {
                    audit.NewValues = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                }
                else if (entry.State == EntityState.Deleted)
                {
                    audit.OldValues = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                }
                else if (entry.State == EntityState.Modified)
                {
                    var modifiedProps = entry.Properties
                    .Where(p => p.IsModified &&
                    !p.Metadata.IsPrimaryKey() &&
                    !ignoredColumns.Contains(p.Metadata.Name) && 
                    !Equals(p.OriginalValue, p.CurrentValue))
                    .ToList();
                                        
                    if (!modifiedProps.Any())
                        continue;

                    audit.OldValues = JsonSerializer.Serialize(
                        modifiedProps.ToDictionary(
                            p => p.Metadata.Name,
                            p => p.OriginalValue?.ToString()));

                    audit.NewValues = JsonSerializer.Serialize(
                        modifiedProps.ToDictionary(
                            p => p.Metadata.Name,
                            p => p.CurrentValue?.ToString()));
                }

                auditLogs.Add(audit);
            }

            await OvmcAuditLogs.AddRangeAsync(auditLogs);
        }
    }
}
