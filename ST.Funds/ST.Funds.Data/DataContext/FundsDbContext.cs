using Microsoft.EntityFrameworkCore;
using ST.Funds.Data.Models;

namespace ST.Funds.Data.DataContext
{
    public class FundsDbContext : DbContext 
    {
        public FundsDbContext(DbContextOptions<FundsDbContext> options)
        : base(options) { }

        public DbSet<Fund> Funds => Set<Fund>();
        public DbSet<FundDocument> Documents => Set<FundDocument>();
        public DbSet<AssetAllocation> AssetAllocations => Set<AssetAllocation>();
        public DbSet<Holding> Holdings => Set<Holding>();
    }
}
