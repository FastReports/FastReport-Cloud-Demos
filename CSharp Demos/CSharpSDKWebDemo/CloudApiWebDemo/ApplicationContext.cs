using CloudApiWebDemo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudApiWebDemo
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Song> Songs { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=demo_song;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Song>().HasData(
                new Song[]
                {
                    new Song() { Id = 1, Author = "Iron Maiden", Alias = "The Number of the Beast", ReleaseDate = new DateTime(1997, 11, 20), Price = 119 },
                    new Song() { Id = 2, Author = "Epica", Alias = "Kingdom of Heaven - A New Age Dawns, Pt. 5", ReleaseDate = new DateTime(2009, 11, 21), Price = 119 },
                    new Song() { Id = 3, Author = "Korol i Shut", Alias = "Will Jump Off the Cliff", ReleaseDate = new DateTime(1999, 10, 28), Price = 149 },
                    new Song() { Id = 4, Author = "Aria", Alias = "Your New World", ReleaseDate = new DateTime(2020, 1, 1), Price = 149 }
                });
        }
    }
}
