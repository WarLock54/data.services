using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public partial class ApplicationDbContext : DbContext
    {
        partial void OnModelCreatingComplete();
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public ApplicationDbContext() : base() { }
        public override int SaveChanges(bool acceptAllChanges)
        {
            CheckReadonlyEntities();
            return base.SaveChanges(acceptAllChanges);
        }
        private void CheckReadonlyEntities()
        {
            var namesOfChangedReadOnlyEntities = this.ChangeTracker.Entries().Where(e => e.Metadata.IsReadOnly() && e.State != EntityState.Unchanged).Select(e => e.Metadata.Name).Distinct().ToList();
            if (namesOfChangedReadOnlyEntities.Any())
            {
                throw new InvalidOperationException($"Attempted to save the following read-only entitie(s): {string.Join(",", namesOfChangedReadOnlyEntities)}");
            }
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Market> Markets { get; set; }
    }
}
