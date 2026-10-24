using Castle.Core.Resource;
using K1XVVD_HSZF_2024251.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K1XVVD_HSZF_2024251.Persistence.MsSql
{
    public class SurvivalDbContext : DbContext
    {
        public DbSet<Character> Characters { get; set; }
        public DbSet<Model.Activity> Activities { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public SurvivalDbContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connStr =
                @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=survivordb;Integrated Security=True;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(connStr);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Skills).HasColumnType("nvarchar(max)");
            });

            modelBuilder.Entity<Model.Activity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.RequiredSkills).HasColumnType("nvarchar(max)");
            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(r => r.Id);
            });
        }
    }

}
