using Microsoft.EntityFrameworkCore;

namespace VPN_RDP_Manager_Web.Models
{
    public class VPNContext : DbContext
    {
        public VPNContext(DbContextOptions<VPNContext> options) : base(options) { }

        public DbSet<Connection> CONNECTIONS { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tablo ismi CONNECTIONS olarak ayarlandı
            modelBuilder.Entity<Connection>().ToTable("CONNECTIONS");
            modelBuilder.Entity<Connection>().HasKey(c => c.SYS_NO);
        }
    }
}
