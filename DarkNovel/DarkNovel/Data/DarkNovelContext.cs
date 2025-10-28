using DarkNovel.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DarkNovel.Data
{
    public class DarkNovelContext : DbContext
    {
        public DarkNovelContext(DbContextOptions<DarkNovelContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CoinPackage> CoinPackages { get; set; }
    }
}
