using Entity.Entities.Invoice;
using Entity.Entities.Masters;
using Entity.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataAccess.Common
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().Where(e => !e.IsOwned()).SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
        public DbSet<UserEntity> UserEntityDBSet { set; get; }
        public DbSet<ItemMasters> ItemMasterDBSet { set; get; }
        public DbSet<InvoiceMaster> InvoiceMasterDBSet { get; set; }
        public DbSet<InvoiceDetails> InvoiceDetailDBSet { get; set; }
        public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
        {
            public ApplicationDBContext CreateDbContext(string[] args)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                var builder = new DbContextOptionsBuilder<ApplicationDBContext>();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                builder.UseSqlServer(connectionString);
                return new ApplicationDBContext(builder.Options);
            }
        }
    }
}
