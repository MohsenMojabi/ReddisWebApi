using Microsoft.EntityFrameworkCore;
using ReddisWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReddisWebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Product[] products = new Product[10000];
            for (int i = 1; i <= 10000; i++)
            {
                products[i - 1] = new Product() { Id = i, Name = "Product #" + i, Price = i * 1000 };
            }
            modelBuilder.Entity<Product>().HasData(products);
        }
    }
}
