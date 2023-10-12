using System;
using System.Reflection.PortableExecutable;
using ControlPanel.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace ControlPanel.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public DbSet<User> Users { get; set; }
        public DbSet<ServerData> Machines { get; set; }
        public AppDbContext(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_configuration.GetConnectionString("ConString"));
        }
    }
}
