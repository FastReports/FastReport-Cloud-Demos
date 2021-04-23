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
        public DbSet<Book> Books { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "Books");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasData(
                new Book[]
                {
                    new Book() { Id = 1, Author = "Marat Alaev", Alias = "How to be a krol", ReleaseDate = new DateTime(2020, 11, 20), Price = 4.20M },
                    new Book() { Id = 2, Author = "Gleb Striukov", Alias = "How to be a bigger krol", ReleaseDate = new DateTime(2020, 11, 21), Price = 4.21M },
                    new Book() { Id = 3, Author = "Sergey Susakov", Alias = "How to survive in army", ReleaseDate = new DateTime(2020, 10, 28), Price = 60.00M },
                    new Book() { Id = 4, Author = "Lev Tolstoi", Alias = "War and piece", ReleaseDate = new DateTime(1867, 1, 1), Price = 3.00M }
                });
        }
    }
}
